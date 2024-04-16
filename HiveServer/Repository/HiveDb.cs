using Microsoft.Extensions.Options;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using MySqlConnector;
using HiveServer;

namespace APIAccountServer.Services;

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
        if(connectionStr == null) // db 연결문자열 가져오기 실패
        {
            return ErrorCode.NullAccountDbConnectionStr;
        }

        _dbConnector = new MySqlConnection(_dbConfig.Value.HiveDb);
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
        // TODO 패스워드 salting






        // TODO 이메일 중복 검사
        var data = _queryFactory.Query("account").Select("email").Where(email).FirstOrDefault();




        // TEST 쌩 패스워드 삽입






        return ErrorCode.None;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}


// Appsettings 파일에 정의된 내용을 이름 그대로 가져온다
public class DbConfig
{

    public string? HiveDb { get; set; }

}
