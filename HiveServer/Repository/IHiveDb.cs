using HiveServer;

namespace HiveServer.Services;

public interface IHiveDb : IDisposable
{
    public Task<ErrorCode> CreateAccountAsync(string id, string password);


}

