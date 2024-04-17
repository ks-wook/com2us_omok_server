
namespace GameAPIServer.Repository;

public class GameDb : IGameDb
{
    public Task<ErrorCode> CreateUserGameData()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<ErrorCode> LoadUserGameData()
    {
        throw new NotImplementedException();
    }
}


public class DbConfig
{
    public string GameDb { get; set; }
}