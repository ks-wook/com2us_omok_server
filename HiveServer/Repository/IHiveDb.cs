using HiveServer;
using HiveServer.Model.DTO;

namespace HiveServer.Services;

public interface IHiveDb : IDisposable
{
    public Task<(ErrorCode, Int64)> CreateAccountAsync(string id, string password);

    public Task<(ErrorCode, Int64)> VerifyUserAndGetAccountIdAsync(string id, string password);
    public Task<(ErrorCode, Int64)> GetAccountIdByIDAsync(string id);
}