namespace HiveServer.Repository;


public interface IMemoryDb
{
    public Task<ErrorCode> InsertHiveLoginTokenAsync(Int64 accountId, string token);

    public Task<(ErrorCode, string)> GetHiveTokenByAccountId(Int64 accountId);
}

