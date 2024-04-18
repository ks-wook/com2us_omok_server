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

    IDbConnection _dbConnector;
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
        _dbConnector.Close();
    }



    public Task<(ErrorCode, UserGameData)> CreateUserGameData(UserGameData userGameData)
    {
        // TODO 새로운 게임 데이터 삽입








        throw new NotImplementedException();
    }

    

    public Task<(ErrorCode, UserGameData)> GetUserGameDataByAccountId(long accountId)
    {
        // TODO accountId를 이용해서 유저 게임 데이터 검색







        
        
        throw new NotImplementedException();
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