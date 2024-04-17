
namespace GameAPIServer.Repository;

public class MemoryDb : IMemoryDb
{
    public Task<(ErrorCode, string)> GetGameTokenByAccountId(long accountId)
    {
        throw new NotImplementedException();
    }

    public Task<ErrorCode> InsertGameLoginTokenAsync(long accountId, string token)
    {
        throw new NotImplementedException();
    }
}

public class MemoryDbConfig
{
    public string HiveRedis { get; set; }
}
