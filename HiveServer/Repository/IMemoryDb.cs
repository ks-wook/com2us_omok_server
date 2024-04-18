using HiveServer.Model.DAO.MemoryDb;

namespace HiveServer.Repository;


public interface IMemoryDb
{
    public Task<ErrorCode> InsertHiveLoginTokenAsync(Int64 accountId, string token);

    public Task<(ErrorCode, LoginToken?)> GetHiveTokenByAccountId(Int64 accountId);
}

