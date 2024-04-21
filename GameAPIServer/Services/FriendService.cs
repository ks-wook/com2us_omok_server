
using GameAPIServer.Model.DAO.GameDb;
using GameAPIServer.Repository;
using ZLogger;

namespace GameAPIServer.Services
{
    public class FriendService : IFriendService
    {
        readonly ILogger<FriendService> _logger;
        readonly IGameDb _gameDb;
        
        public FriendService(IGameDb gameDb, ILogger<FriendService> logger)
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

            result = await _gameDb.CreateFriend(uid, friendUid);


            return result;
        }


        public async Task<ErrorCode> AcceptFriendReq(Int64 uid, Int64 friendUid)
        {
            // uid와 frienduid가 일치하는 친구 요청이 있었는지 확인
            dynamic? data = await _gameDb.GetFriemdDataByUidAndFriendUid(uid, friendUid);
            if (data == null)
            {
                _logger.ZLogError
                    ($"[DeleteFriend] ErrorCode: {ErrorCode.NullFriendData}, uid: {uid}, friendUid: {friendUid}");
                return ErrorCode.NullFriendData;
            }

            // 일치하는 친구 요청이 있었던 경우에만 해당 데이터의 friendyn값이 false인 경우에만 true로 변경
            ErrorCode result = await _gameDb.AcceptFriendReq(uid, friendUid);

            return result;
        }

        
        public async Task<ErrorCode> DeleteFriend(Int64 uid, Int64 friendUid)
        {
            // uid와 frienduid가 일치하는 친구 요청이 있었는지 확인
            dynamic? data = await _gameDb.GetFriemdDataByUidAndFriendUid(uid, friendUid);
            if(data == null)
            {
                _logger.ZLogError
                    ($"[DeleteFriend] ErrorCode: {ErrorCode.NullFriendData}, uid: {uid}, friendUid: {friendUid}");
                return ErrorCode.NullFriendData;
            }


            // 일치하는 친구 요청이 있으면서 friendyn이 false인 경우에 해당 데이터를 삭제
            ErrorCode result = await _gameDb.DeleteFriend(uid, friendUid);

            return result;
        }

        public async Task<ErrorCode> RejectFriendReq(Int64 uid, Int64 friendUid)
        {
            // 친구가 아닌 경우만 친구거절이 가능
            (ErrorCode result, Friend? data) = await _gameDb.GetFriemdDataByUidAndFriendUid(uid, friendUid);
            if (data == null)
            {
                _logger.ZLogError
                    ($"[RejectFriendReq] ErrorCode: {ErrorCode.NullFriendData}, uid: {uid}, friendUid: {friendUid}");
                return ErrorCode.NullFriendData;
            }

            if(data.friend_yn == true) // 이미 친구인경우
            {
                _logger.ZLogError
                    ($"[RejectFriendReq] ErrorCode: {ErrorCode.FailRejectFriendReq}, uid: {uid}, friendUid: {friendUid}");
                return ErrorCode.NullFriendData;
            }

            result = await _gameDb.DeleteFriend(uid, friendUid);

            return result;
        }


    }
}
