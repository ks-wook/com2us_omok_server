using Microsoft.Extensions.Options;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using MySqlConnector;
using HiveServer;
using Microsoft.AspNetCore.SignalR.Protocol;
using HiveServer.Model.DAO;
using ZLogger;

namespace HiveServer.Services;

public class HiveDb : IHiveDb
{
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<HiveDb> _logger;

    IDbConnection _dbConnector;
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

            Console.WriteLine("연결 문자열이 존재하지 않음.");
            return ErrorCode.NullAccountDbConnectionStr;
        }

        _dbConnector = new MySqlConnection(connectionStr);
        _dbConnector.Open();

        return ErrorCode.None;
    }


    // disconnect with db
    private void DbDisconnect()
    {
        _dbConnector.Close();
    }



    public async Task<ErrorCode> CreateAccountAsync(string email, string password)
    {
        try
        {
            // 이메일 중복 check
            dynamic? data = await _queryFactory.Query("account")
                .Where("email", "=", email).FirstOrDefaultAsync();

            if (data != null)
            {
                return ErrorCode.DuplicatedEmail;
            }


            // 패스워드 salting
            string saltValue, hashedPassword;
            (saltValue, hashedPassword) = HiveServerSequrity.HashPasswordWithSalt(password);

            // 계정 정보 삽입
            var insertSuccess = await _queryFactory.Query("account").InsertAsync(new
            {
                Email = email,
                Password = hashedPassword,
                SaltValue = saltValue,
            });

            _logger.ZLogDebug(
                $"[CreateAccount] email: {email}, salt_value : {saltValue}, hashed_pw:{hashedPassword}");

            if (insertSuccess != 1)
            {
                _logger.ZLogError(
                    $"[CreateAccount] ErrorCode: {ErrorCode.InsertAccountFail}");
                return ErrorCode.InsertAccountFail;
            }

        }
        catch (Exception e) 
        {
            _logger.ZLogError(
                $"[CreateAccount] ErrorCode: {ErrorCode.CreateAccountFail}");
            return ErrorCode.CreateAccountFail;
        }
        
        // 하이브 계정 생성 성공
        return ErrorCode.None;
    }

    // distructor
    public void Dispose()
    {
        DbDisconnect();
    }
}


// Appsettings 파일에 정의된 내용을 이름 그대로 가져온다
public class DbConfig
{
    public string HiveDb { get; set; }
}
