using GameAPIServer.Model.DAO.GameDb;

namespace GameAPIServer.Repository;

public interface IGameDb : IDisposable
{
    // 게임 데이터 생성 시 유저 계정에 묶인 accountId 가 있어야 한다.
    public Task<(ErrorCode, UserGameData?)> CreateUserGameData(Int64 accountId);
    public Task<(ErrorCode, UserGameData?)> GetUserGameDataByAccountId(Int64 accountId);

}
