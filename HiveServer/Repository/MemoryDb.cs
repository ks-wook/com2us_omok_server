﻿namespace HiveServer.Repository;

using CloudStructures;
using CloudStructures.Structures;
using HiveServer.Model.DAO.MemoryDb;
using HiveServer.Services;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using ZLogger;

public class MemoryDb : IMemoryDb
{
    readonly ILogger<MemoryDb> _logger;
    readonly IOptions<MemoryDbConfig> _momoryDbConfig;

    RedisConnection _redisConnector;

    public MemoryDb(ILogger<MemoryDb> logger, IOptions<MemoryDbConfig> momoryDbConfig)
    {
        _logger = logger;
        _momoryDbConfig = momoryDbConfig;
        RedisConfig rc = new RedisConfig("defalut", _momoryDbConfig.Value.HiveRedis);

        _redisConnector = new RedisConnection(rc);
    }

    // 로그인 유저 토큰 삽입
    public async Task<ErrorCode> InsertHiveLoginTokenAsync(Int64 uid, string token)
    {
        var key = MemoryDbKeyGenerator.GenLoginTokenKey(uid.ToString());

        LoginToken loginToken = new LoginToken()
        {
            Uid = uid,
            Token = token,
        };

        try
        {
            // 토큰, 만료시간 설정
            RedisString<LoginToken> redis = new RedisString<LoginToken>(
                _redisConnector, 
                key, 
                TimeSpan.FromMinutes(HiveDefine.LoginTokenExpireMin));

            if (await redis.SetAsync(loginToken) == false)
            {
                _logger.ZLogError
                    ($"[InsertHiveLoginTokenAsync] Uid:{uid}, Token:{token} ErrorCode: {ErrorCode.HiveLoginTokenRedisFail}");
                return ErrorCode.HiveLoginTokenRedisFail;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError
                    ($"[InsertHiveLoginTokenAsync] Uid:{uid}, Token:{token} ErrorCode: {ErrorCode.HiveRedisConnectionFail}");
            return ErrorCode.HiveRedisConnectionFail;
        }
    }

    // 유저 아이디로 유효 토큰 검색
    public async Task<(ErrorCode, LoginToken?)> GetHiveTokenByUid(long uid)
    {
        // accountId를 이용해서 토큰을 검색한 후 검색된 반환한다.
        var key = MemoryDbKeyGenerator.GenLoginTokenKey(uid.ToString());

        try
        {
            RedisString<LoginToken> redis = new(_redisConnector, key, null);
            RedisResult<LoginToken> loginToken = await redis.GetAsync();
            if (!loginToken.HasValue)
            {
                _logger.ZLogError(
                    $"[GetHiveTokenByAccountId] UID = {key}, Invalid Token");
                return (ErrorCode.NullHiveLoginToken, null);
            }

            return (ErrorCode.None, loginToken.Value);
        }
        catch
        {
            _logger.ZLogError
                ($"[GetHiveTokenByAccountId] UID:{uid}, Fail Get Hive Login Token");
            return (ErrorCode.NullHiveLoginToken, null);
        }

    }
}




public class MemoryDbConfig
{
    public string HiveRedis { get; set; } = string.Empty;
}
