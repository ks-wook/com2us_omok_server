namespace GameAPIServer.Services;

public interface IGameService
{
    // TODO 게임 데이터 관련 로직작성에 필요한 함수 선언
    public Task<(ErrorCode, int)> InitNewUserGameData(Int64 accountId, string nickname);

}
