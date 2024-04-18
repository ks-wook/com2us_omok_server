using CloudStructures;
using CloudStructures.Structures;
using GameAPIServer.Model.DAO.MemoryDb;
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


    public Task<(ErrorCode, string)> GetGameTokenByAccountId(Int64 accountId)
    {
        // TODO gameredis에서 토큰 검색











        throw new NotImplementedException();
    }

    // 하이브에서 인증된 로그인 토큰을 game redis에 삽입
    public async Task<ErrorCode> InsertGameLoginTokenAsync(Int64 accountId, string token)
    {
        var key = MemoryDbKeyGenerator.GenLoginTokenKey(accountId.ToString());

        LoginToken loginToken = new LoginToken()
        {
            Uid = accountId,
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
                    ($"[InsertGameLoginTokenAsync] Uid:{accountId}, Token:{token} ErrorCode: {ErrorCode.GameLoginTokenRedisFail}");
                return ErrorCode.NullGameLoginToken;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError
                    ($"[InsertGameLoginTokenAsync] Uid:{accountId}, Token:{token} ErrorCode: {ErrorCode.GameRedisConnectionFail}");
            return ErrorCode.GameRedisConnectionFail;
        }
    }
}

public class MemoryDbConfig
{
    public string GameRedis { get; set; } = string.Empty;
}
