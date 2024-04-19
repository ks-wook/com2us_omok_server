
using GameAPIServer.Repository;

namespace GameAPIServer.Services
{
    public class FriendService : IFriendService
    {
        readonly ILogger _logger;
        readonly IGameDb _gameDb;
        
        public FriendService(IGameDb gameDb, ILogger logger)
        {
            _gameDb = gameDb;
            _logger = logger;
        }

        public Task<ErrorCode> AddFriendReq(Int64 uid, Int64 friendUid)
        {
            // TODO uid, friendUid에 대하여 전부 존재하는 유저인지 검색




            // TODO 둘 모두 존재하는 경우에만 친구 추가 시도






            throw new NotImplementedException();
        }


        public Task<ErrorCode> AcceptFriendReq(long uid, long friendUid)
        {
            // TODO 친구 관계 수락






            throw new NotImplementedException();
        }

        
        public Task<ErrorCode> DeleteFriend(long uid, long friendUid)
        {
            // TODO 친구 관계 거절







            throw new NotImplementedException();
        }

        public Task<ErrorCode> RejectFriendReq(long uid, long friendUid)
        {
            // TODO 친구 관계 삭제 (이미 친구인 경우만 가능)







            throw new NotImplementedException();
        }
    }
}
