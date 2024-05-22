using Microsoft.Extensions.Options;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using MySqlConnector;
using HiveServer;
using Microsoft.AspNetCore.SignalR.Protocol;
using ZLogger;
using HiveServer.Model.DTO;
using HiveServer.Model.DAO.HiveDb;

namespace HiveServer.Services;

public class HiveDb : IHiveDb
{
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<HiveDb> _logger;

    IDbConnection? _dbConnector;
    SqlKata.Compilers.MySqlCompiler _dbCompiler;
    QueryFactory _queryFactory;

    public HiveDb(ILogger<HiveDb> logger, IOptions<DbConfig> dbConfig)
    {
        _dbConfig = dbConfig;
        _logger = logger;

        DbConnect();

        _dbCompiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConnector, _dbCompiler);
    }

    // create connection with db
    private ErrorCode DbConnect()
    {
        string? connectionStr = _dbConfig.Value.HiveDb;
        if (connectionStr == null) // db 연결문자열 가져오기 실패
        {
            _logger.ZLogError(
                $"[DbConnect] Null Db Connection String");

            return ErrorCode.NullAccountDbConnectionStr;
        }

        _dbConnector = new MySqlConnection(connectionStr);
        _dbConnector.Open();

        return ErrorCode.None;
    }

    // disconnect with db
    private void DbDisconnect()
    {
        if (_dbConnector == null)
        {
            _logger.ZLogError
                ($"[ErrorCode]: {ErrorCode.FailDisconnectHiveDb}");
            return;
        }
        _dbConnector.Close();
    }


    // 하이브 계정 생성
    public async Task<(ErrorCode, Int64)> CreateAccountAsync(string id, string password)
    {
        try
        {
            // 아이디 중복 check
            dynamic? data = await _queryFactory.Query("account")
                .Where("id", "=", id).FirstOrDefaultAsync();

            if (data != null)
            {
                return (ErrorCode.DuplicatedEmail, -1);
            }

            // 패스워드 salting
            string saltValue, hashedPassword;
            (saltValue, hashedPassword) = HiveServerSequrity.HashingWithSaltValue(password);


            // 계정 정보 삽입
            var insertSuccess = await _queryFactory.Query("account").InsertAsync(new
            {
                Id = id,
                Password = hashedPassword,
                SaltValue = saltValue,
            });


            _logger.ZLogDebug
                ($"[CreateAccount] id: {id}, salt_value : {saltValue}, hashed_pw:{hashedPassword}");

            if (insertSuccess != 1)
            {
                _logger.ZLogError(
                    $"[CreateAccount] ErrorCode: {ErrorCode.InsertAccountFail}");
                return (ErrorCode.InsertAccountFail, -1);
            }

            return await GetAccountIdByIDAsync(id);
        }
        catch
        {
            _logger.ZLogError(
                $"[CreateAccount] ErrorCode: {ErrorCode.CreateAccountFail}");
            return (ErrorCode.CreateAccountFail, -1);
        }
    }

    // 하이브 계정 로그인
    public async Task<(ErrorCode, Int64)> VerifyUserAndGetAccountIdAsync(string id, string password)
    {
        try
        {
            // 하이브 계정 검색
            Account? data = await _queryFactory.Query("account")
                .Where("id", id).FirstOrDefaultAsync<Account>();
            if(data == null) // 하이브 계정이 존재하지 않는 경우
            {
                return (ErrorCode.InvalidAccountEmail, -1);
            }

            // Base64 문자열을 바이트 배열로 디코딩 -> 전달된 패스워드로 해시값 재생성
            string base64String = data.saltValue;
            byte[] saltByteArray = Convert.FromBase64String(base64String);

            string hashedPassword;
            (_, hashedPassword) = HiveServerSequrity.HashingWithSaltValue(saltByteArray, password);

            // 검색된 계정의 해시값과 방금 생성한 해시값 비교
            if(data.password != hashedPassword) // 비밀번호 불일치
            {
                return (ErrorCode.IDOrPasswordMismatch, -1);
            }

            // 계정정보 탐색 성공
            return (ErrorCode.None, data.account_id);
        }
        catch
        {
            _logger.LogError
                ($"[VerifyUserAndGetAccountIdAsync] ErrorCode: {ErrorCode.FailVerifyUser}");
            return (ErrorCode.HiveLoginFail, -1);
        }
    }
    public async Task<(ErrorCode, Int64)> GetAccountIdByIDAsync(string id)
    {
        try
        {
            // 하이브 계정 검색
            Account? data = await _queryFactory.Query("account")
                .Where("id", id).FirstOrDefaultAsync<Account>();
            if (data == null) // 하이브 계정이 존재하지 않는 경우
            {
                return (ErrorCode.InvalidAccountEmail, -1);
            }

            return (ErrorCode.None, data.account_id);
        }
        catch
        {
            _logger.LogError
                ($"[GetAccountIdByIDAsync] ErrorCode: {ErrorCode.NullAccountData}");
            return (ErrorCode.HiveLoginFail, -1);
        }
    }

    // distructor
    public void Dispose()
    {
        DbDisconnect();
    }
}

public class DbConfig
{
    public string HiveDb { get; set; } = string.Empty;
}
