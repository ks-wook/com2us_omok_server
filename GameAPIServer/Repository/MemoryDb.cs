using CloudStructures;
using CloudStructures.Structures;
using GameAPIServer.Model.DAO.MemoryDb;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using ZLogger;

namespace GameAPIServer.Repository;

public class MemoryDb : IMemoryDb
{
    readonly ILogger<MemoryDb> _logger;
    readonly IOptions<MemoryDbConfig> _momoryDbConfig;

    RedisConnection _redisConnector;

    public MemoryDb(ILogger<MemoryDb> logger, IOptions<MemoryDbConfig> momoryDbConfig)
    {
        _logger = logger;
        _momoryDbConfig = momoryDbConfig;
        RedisConfig rc = new RedisConfig("defalut", _momoryDbConfig.Value.GameRedis);

        _redisConnector = new RedisConnection(rc);
    }

    public async Task<(ErrorCode, LoginToken?)> GetGameTokenByUidAsync(Int64 uid)
    {
        // uid 이용해서 토큰을 검색한 후 검색된 반환한다.
        var key = MemoryDbKeyGenerator.GenLoginTokenKey(uid.ToString());

        try
        {
            RedisString<LoginToken> redis = new(_redisConnector, key, null);
            RedisResult<LoginToken> loginToken = await redis.GetAsync();
            if (!loginToken.HasValue)
            {
                _logger.ZLogError(
                    $"[GetGameTokenByUid] UID = {key}, Invalid Token");
                return (ErrorCode.NullGameLoginToken, null);
            }

            return (ErrorCode.None, loginToken.Value);
        }
        catch
        {
            _logger.ZLogError
                ($"[GetGameTokenByUid] UID:{uid}, Fail Get Hive Login Token");
            return (ErrorCode.NullGameLoginToken, null);
        }
    }

    // 하이브에서 인증된 로그인 토큰을 game redis에 삽입
    public async Task<ErrorCode> InsertGameLoginTokenAsync(Int64 uid, string token)
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
                TimeSpan.FromMinutes(GameDefine.LoginTokenExpireMin));

            if (await redis.SetAsync(loginToken) == false)
            {
                _logger.ZLogError
                    ($"[InsertGameLoginTokenAsync] Uid:{uid}, Token:{token} ErrorCode: {ErrorCode.GameLoginTokenRedisFail}");
                return ErrorCode.NullGameLoginToken;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError
                    ($"[InsertGameLoginTokenAsync] Uid:{uid}, Token:{token} ErrorCode: {ErrorCode.GameRedisConnectionFail}");
            return ErrorCode.GameRedisConnectionFail;
        }
    }

    public async Task<ErrorCode> SetUserReqLockAsync(string userLockKey)
    {
        try
        {
            RedisString<LoginToken> redis = new(_redisConnector, userLockKey, TimeSpan.FromMinutes(GameDefine.ReqLockExpireMin));
            
            if (await redis.SetAsync(new LoginToken(), null, StackExchange.Redis.When.NotExists) == false)
            {
                return ErrorCode.FailSetUserLockKey; // 락이 걸려 있는 경우
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError($"[SetUserReqLockAsync] UserLockKey = {userLockKey}");
            return ErrorCode.FailSetUserLockKey;
        }
    }

    public async Task<ErrorCode> DelUserReqLockAsync(string userLockKey)
    {
        try
        {
            RedisString<LoginToken> redis = new(_redisConnector, userLockKey, null);

            if(await redis.DeleteAsync() == false)
            {
                return ErrorCode.FailDelUserLockKey;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError($"[DelUserReqLockAsync] UserLockKey = {userLockKey}");
            return ErrorCode.FailDelUserLockKey;
        }
    }

}

public class MemoryDbConfig
{
    public string GameRedis { get; set; } = string.Empty;
}
