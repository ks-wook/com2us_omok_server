
using GameAPIServer.Model.DAO.GameDb;
using GameAPIServer.Repository;
using Microsoft.Extensions.Configuration.UserSecrets;
using ZLogger;

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

    

    public Task<(ErrorCode, UserGameData?)> InitNewUserGameData(Int64 accountId)
    {
        // 게임을 새로 시작한 유저의 데이터를 만들고 반환
        UserGameData userGameData = new UserGameData()
        {
            account_id = accountId,
            nickname = "User" + accountId,
        };

        try
        {
            // TODO db 삽입 시도








        }
        catch(Exception e)
        {
            




        }


        throw new NotImplementedException();
    }

    public async Task<(ErrorCode, UserGameData?)> GetGameDataByAccountId(Int64 accountId)
    {

        // accountId를 이용하여 유저 게임 데이터 검색
        (ErrorCode result, UserGameData? userGameData) = 
            await _gameDb.GetUserGameDataByAccountId(accountId);


        if(result != ErrorCode.None) // 데이터 검색에서 오류
        {
            _logger.ZLogError
                ($"[GetGameDataByAccountId] ErrorCode: {result}");
            return (result, null);

        }
        else if(userGameData == null) // 게임 데이터가 존재하지 않는 경우
        {
            _logger.ZLogInformation
                ($"[GetGameDataByAccountId] Not exist UserGameData");

            // 새로운 게임 데이터 생성
            (result, userGameData) = await InitNewUserGameData(accountId);

        }

        return (result, userGameData);
        
    }
}
