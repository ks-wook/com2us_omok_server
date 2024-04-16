﻿using Microsoft.Extensions.Options;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using MySqlConnector;

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



    public Task<ErrorCode> CreateAccount(string id, string password)
    {
        throw new NotImplementedException();
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
