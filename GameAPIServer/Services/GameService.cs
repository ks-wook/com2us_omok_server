
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

    public async Task<(ErrorCode, UserGameData?)> InitNewUserGameData(Int64 accountId)
    {
        // 게임을 새로 시작한 유저의 데이터를 만들고 반환
        try
        {
            (ErrorCode result, UserGameData? data) = await _gameDb.CreateUserGameData(accountId);

            if(result != ErrorCode.None || data == null) 
            {
                return (result, null);
            }

            return (result, data);
        }
        catch
        {
            _logger.ZLogError
               ($"[CreateUserGameData] ErrorCode: {ErrorCode.FailCreateNewGameData}, accountId: {accountId}");
            return (ErrorCode.FailCreateNewGameData, null);
        }

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
            result = ErrorCode.NullUserGameData;
            return (result, null);
        }

        return (result, userGameData);
    }

    public async Task<(ErrorCode, UserGameData?)> GetUserGameDataByUid(Int64 uid)
    {
        // uid를 이용하여 유저 게임 데이터 검색
        (ErrorCode result, UserGameData? userGameData) =
            await _gameDb.GetGameDataByUid(uid);

        if (result != ErrorCode.None) // 데이터 검색에서 오류
        {
            _logger.ZLogError
                ($"[GetGameDataByAccountId] ErrorCode: {result}");
            return (result, null);

        }
        else if (userGameData == null) // 게임 데이터가 존재하지 않는 경우
        {
            _logger.ZLogInformation
                ($"[GetGameDataByAccountId] Not exist UserGameData");
            result = ErrorCode.NullUserGameData;
            return (result, null);
        }

        return (result, userGameData);
    }

    public async Task<ErrorCode> UpdateGameDataRecentLoginByUid(Int64 uid)
    {
        // 최근 로그인 시간 업데이트
        ErrorCode result = await _gameDb.UpdateGameDataRecentLoginByUid(uid);

        if(result != ErrorCode.None)
        {
            _logger.ZLogError
                ($"[UpdateGameDataRecentLoginByUid] ErrorCode: {ErrorCode.FailUpdateRecentLogin}, uid: {uid}");
            return ErrorCode.FailUpdateRecentLogin;
        }

        return result;
    }

    public async Task<(ErrorCode, IEnumerable<Item>?)> GetItemListByUid(Int64 uid)
    {
        (ErrorCode result, IEnumerable<Item>? itemList) = 
            await _gameDb.GetItemListByUid(uid);

        if (result != ErrorCode.None) // 아이템 검색에서 오류
        {
            _logger.ZLogError
                ($"[GetGameDataByAccountId] ErrorCode: {result}");
            return (result, null);
        }

        return (result,  itemList);
    }

    public async Task<UserGameData?> LoadGameDataByUserId(Int64 uid)
    {
        // accountId 값을 사용하여 게임 데이터를 검색
        (ErrorCode result, UserGameData? userGameData) = await GetUserGameDataByUid(uid);

        if (userGameData == null) // UserGameData가 존재하지 않는 경우
        {
            _logger.ZLogError
                ($"[LoadGameDataByAccountId] ErrorCode: {ErrorCode.NullUserGameData}");
            return null;
        }

        // 로그인 시간 업데이트
        result = await UpdateGameDataRecentLoginByUid(userGameData.uid);

        return userGameData;
    }

    public async Task<(ErrorCode, long)> GetUserIdByAccountId(long accountId)
    {
        // accountId 값을 사용하여 게임 데이터를 검색
        (ErrorCode result, UserGameData? userGameData) = await GetGameDataByAccountId(accountId);

        if (userGameData == null) // UserGameData가 존재하지 않는 경우
        {
            _logger.ZLogError
                ($"[GetUserIdByAccountId] ErrorCode: {ErrorCode.NullUserGameData}");
            return (ErrorCode.NullUserGameData, -1);
        }

        return (ErrorCode.None, userGameData.uid);
    }
}
