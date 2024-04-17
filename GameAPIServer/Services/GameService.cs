
using GameAPIServer.Repository;

namespace GameAPIServer.Services;

public class GameService : IGameService
{
    readonly ILogger<GameService> _logger;
    readonly IGameDb _gameDb;
    readonly IMemoryDb _memoryDb;

    public GameService(ILogger<GameService> logger, IGameDb gameDb, IMemoryDb memoryDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _memoryDb = memoryDb;
    }   

    public Task<(ErrorCode, int)> InitNewUserGameData(long accountId, string nickname)
    {
        // TODO 최초 게임 시작한 유저에게 기본 데이터 db에 삽입후 결과 반환




        throw new NotImplementedException();
    }
}
