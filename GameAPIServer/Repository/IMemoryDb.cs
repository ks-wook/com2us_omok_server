namespace GameAPIServer.Repository;

public interface IMemoryDb
{
    public Task<ErrorCode> InsertGameLoginTokenAsync(Int64 accountId, string token);
    public Task<(ErrorCode, string)> GetGameTokenByAccountId(Int64 accountId);

}
