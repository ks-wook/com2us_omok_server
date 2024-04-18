
namespace GameAPIServer.Repository;

public class GameDb : IGameDb
{
    public Task<ErrorCode> CreateUserGameData()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        
    }

    public Task<ErrorCode> LoadUserGameData()
    {
        throw new NotImplementedException();
    }
}


public class DbConfig
{
    public string GameDb { get; set; } = string.Empty;
}