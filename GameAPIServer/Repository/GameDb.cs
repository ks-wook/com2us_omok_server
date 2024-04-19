using GameAPIServer.Model.DAO.GameDb;
using Microsoft.Extensions.Options;
using System.Data;
using MySqlConnector;
using SqlKata;
using ZLogger;
using SqlKata.Execution;

namespace GameAPIServer.Repository;

public class GameDb : IGameDb
{
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<GameDb> _logger;

    IDbConnection? _dbConnector;
    SqlKata.Compilers.MySqlCompiler _dbCompiler;
    QueryFactory _queryFactory;

    public GameDb(ILogger<GameDb> logger, IOptions<DbConfig> dbConfig)
    {
        _dbConfig = dbConfig;
        _logger = logger;

        _dbCompiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConnector, _dbCompiler);
    }


    // create connection with db
    private ErrorCode DbConnect()
    {
        string? connectionStr = _dbConfig.Value.GameDb;
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
        if(_dbConnector == null) 
        {
            _logger.ZLogError
                ($"[ErrorCode]: {ErrorCode.FailDisconnectGameDb}");
            return;
        }
        _dbConnector.Close();
    }



    public async Task<(ErrorCode, UserGameData?)> CreateUserGameData(Int64 accountId)
    {
        // 새로운 게임 데이터 삽입
        UserGameData? userGameData = new UserGameData()
        {
            account_id = accountId,
            nickname = "User" + accountId,
        };

        var insertSuccess = await _queryFactory.Query("user_game_data")
            .InsertAsync(userGameData);

        _logger.ZLogDebug
            ($"[CreateUserGameData] accountId: {accountId}, nickname: {userGameData.nickname}");


        if(insertSuccess != 1)
        {
            _logger.ZLogError
                ($"[CreateUserGameData] ErrorCode: {ErrorCode.FailCreateNewGameData}, " +
                 $"accountId: {accountId}, nickname: {userGameData.nickname}");
            return (ErrorCode.FailCreateNewGameData, null);
        }


        // 데이터 삽입 성공, 삽입된 게임데이터 획득
        (ErrorCode result, userGameData) = await GetUserGameDataByAccountId(accountId);

        return (result, userGameData);
    }



    // AccountId를 이용해서 UserGameData Search
    public async Task<(ErrorCode, UserGameData?)> GetUserGameDataByAccountId(Int64 accountId)
    {
        UserGameData? data = await _queryFactory.Query("user_game_data")
            .Where("account_id", accountId).FirstOrDefaultAsync<UserGameData>();

        if (data == null) // 하이브 계정이 존재하지 않는 경우
        {
            return (ErrorCode.NullUserGameData, null);
        }

        return (ErrorCode.None, data);
    }

    public void Dispose()
    {
        DbDisconnect();
    }
}


public class DbConfig
{
    public string GameDb { get; set; } = string.Empty;
}