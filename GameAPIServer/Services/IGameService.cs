using GameAPIServer.Model.DAO.GameDb;

namespace GameAPIServer.Services;

public interface IGameService
{
    // UserGameData ~ 
    public Task<(ErrorCode, UserGameData?)> InitNewUserGameData(Int64 accountId);
    public Task<(ErrorCode, UserGameData?)> GetGameDataByAccountId(Int64 accountId);
    public Task<(ErrorCode, UserGameData?)> GetGameDataByUid(Int64 uid);
    public Task<ErrorCode> UpdateGameDataRecentLoginByUid(Int64 uid);
    public Task<UserGameData?> LoadGameData(Int64 accountId);

    // Item ~
    public Task<(ErrorCode, IEnumerable<Item>?)> GetItemListByUid(Int64 uid);


}
