using HiveServer.Model.DAO.MemoryDb;

namespace HiveServer.Repository;


public interface IMemoryDb
{
    public Task<ErrorCode> InsertHiveLoginTokenAsync(Int64 uid, string token);

    public Task<(ErrorCode, LoginToken?)> GetHiveTokenByUid(Int64 uid);
}