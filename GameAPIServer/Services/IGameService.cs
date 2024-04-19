using GameAPIServer.Model.DAO.GameDb;

namespace GameAPIServer.Services;

public interface IGameService
{
    // 게임 데이터 관련 로직작성에 필요한 함수 선언
    public Task<(ErrorCode, UserGameData?)> InitNewUserGameData(Int64 accountId);
    public Task<(ErrorCode, UserGameData?)> GetGameDataByAccountId(Int64 accountId);

}
