
using GameAPIServer.Model.DAO.GameDb;
using GameAPIServer.Repository;
using ZLogger;

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

        public async Task<ErrorCode> AddFriendReq(Int64 uid, Int64 friendUid)
        {
            // uid, friendUid에 대하여 전부 존재하는 유저인지 검색
            (ErrorCode result, UserGameData? userGameData) = await _gameDb.GetGameDataByUid(uid);
            if(result != ErrorCode.None || userGameData == null)
            {
                _logger.ZLogError
                    ($"[AddFriendReq] ErrorCode : {ErrorCode.NullUserGameData}, Uid: {uid}");
            }

            // 친구요청 상대에 대해서도 동일하게 검사
            (result, userGameData) = await _gameDb.GetGameDataByUid(friendUid);
            if (result != ErrorCode.None || userGameData == null)
            {
                _logger.ZLogError
                    ($"[AddFriendReq] ErrorCode : {ErrorCode.NullUserGameData}, Uid: {uid}");
            }

            // 둘 모두 존재하는 경우에만 친구 추가 시도
            Friend data = new Friend() 
            { 
                uid = uid,
                friend_uid = friendUid,
            };

            int insertSuccess;



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
