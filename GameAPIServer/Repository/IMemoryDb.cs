using GameAPIServer.Model.DAO.MemoryDb;

namespace GameAPIServer.Repository;

public interface IMemoryDb
{
    public Task<(ErrorCode, LoginToken?)> GetGameTokenByUidAsync(Int64 uid);
    public Task<ErrorCode> InsertGameLoginTokenAsync(Int64 uid, string token);
    public Task<ErrorCode> SetUserReqLockAsync(string userLockKey);
    public Task<ErrorCode> DelUserReqLockAsync(string userLockKey);
}