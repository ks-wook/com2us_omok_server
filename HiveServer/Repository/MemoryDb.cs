namespace HiveServer.Repository;

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
    public async Task<ErrorCode> InsertHiveLoginTokenAsync(long accountId, string token)
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
                TimeSpan.FromMinutes(Define.LoginTokenExpireMin));

            if (await redis.SetAsync(loginToken) == false)
            {
                _logger.ZLogError
                    ($"[InsertHiveLoginTokenAsync] Uid:{accountId}, Token:{token} ErrorCode: {ErrorCode.LoginTokenRedisFail}");
                return ErrorCode.LoginTokenRedisFail;
            }

        }
        catch
        {
            _logger.ZLogError
                    ($"[InsertHiveLoginTokenAsync] Uid:{accountId}, Token:{token} ErrorCode: {ErrorCode.RedisConnectionFail}");
            return ErrorCode.RedisConnectionFail;
        }

        return ErrorCode.None;
    }



    // 유저 accountId로 유효 토큰 검색
    public async Task<(ErrorCode, string)> GetHiveTokenByAccountId(long accountId)
    {
        // accountId를 이용해서 토큰을 검색한 후 검색된 반환한다.
        var key = MemoryDbKeyGenerator.GenLoginTokenKey(accountId.ToString());

        try
        {
            RedisString<LoginToken> redis = new(_redisConnector, key, null);
            RedisResult<LoginToken> loginToken = await redis.GetAsync();
            if (!user.HasValue)
            {
                _logger.ZLogError(
                    $"[GetHiveTokenByAccountId] UID = {key}, Invalid Token");
                return (false, null);
            }

            return (true, user.Value);
        }
        catch
        {
            _logger.ZLogError($"[GetUserAsync] UID:{uid},ErrorMessage:ID does Not Exist");
            return (false, null);
        }

        throw new NotImplementedException();
    }

}




public class MemoryDbConfig
{
    public string HiveRedis { get; set; } = string.Empty;
}
