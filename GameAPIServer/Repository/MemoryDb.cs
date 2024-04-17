
namespace GameAPIServer.Repository;

public class MemoryDb : IMemoryDb
{
    public Task<(ErrorCode, string)> GetGameTokenByAccountId(long accountId)
    {
        // TODO gameredis에서 토큰 검색



        throw new NotImplementedException();
    }

    public Task<ErrorCode> InsertGameLoginTokenAsync(long accountId, string token)
    {
        // TODO 게임 로그인 토큰 redis로 삽입


        throw new NotImplementedException();
    }
}

public class MemoryDbConfig
{
    public string HiveRedis { get; set; }
}
