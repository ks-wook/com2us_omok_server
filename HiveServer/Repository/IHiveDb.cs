using HiveServer;
using HiveServer.Model.DTO;

namespace HiveServer.Services;

public interface IHiveDb : IDisposable
{
    public Task<ErrorCode> CreateAccountAsync(string id, string password);

    public Task<LoginRes> VerifyUserAsync(string email, string password);
}

