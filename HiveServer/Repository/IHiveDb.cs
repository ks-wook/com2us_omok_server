using HiveServer;

namespace APIAccountServer.Services;

public interface IHiveDb : IDisposable
{
    public Task<ErrorCode> CreateAccountAsync(string id, string password);


}

