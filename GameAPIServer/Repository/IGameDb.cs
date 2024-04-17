namespace GameAPIServer.Repository;

public interface IGameDb : IDisposable
{
    // TODO GameData 관련 기능개발


    public Task<ErrorCode> CreateUserGameData();
    public Task<ErrorCode> LoadUserGameData();

}
