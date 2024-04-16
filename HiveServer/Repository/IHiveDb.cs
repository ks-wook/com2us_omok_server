namespace APIAccountServer.Services;

public interface IHiveDb : IDisposable
{
    public Task<ErrorCode> CreateAsync(string id, string password);


}

