namespace APIAccountServer.Services;

public interface IAccountDb : IDisposable
{
    public Task<ErrorCode> CreateAsync(string id, string password);


}

