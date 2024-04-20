using GameAPIServer.Model.DAO.GameDb;

namespace GameAPIServer.Services;

public interface IGameService
{
    // UserGameData ~ 
    public Task<(ErrorCode, UserGameData?)> InitNewUserGameData(Int64 accountId);
    public Task<(ErrorCode, UserGameData?)> GetGameDataByAccountId(Int64 accountId);
    public Task<(ErrorCode, UserGameData?)> GetGameDataByUid(Int64 uid);

}
