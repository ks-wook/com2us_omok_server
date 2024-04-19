namespace GameAPIServer.Services
{
    public interface IFriendService
    {
        // 친구 관계 추가
        public Task<ErrorCode> AddFriendReq(Int64 uid, Int64 friendUid);

        // 친구 관계 수락, 거절
        public Task<ErrorCode> AcceptFriendReq(Int64 uid, Int64 friendUid);
        public Task<ErrorCode> RejectFriendReq(Int64 uid, Int64 friendUid);

        // 친구관계 삭제
        public Task<ErrorCode> DeleteFriend(Int64 uid, Int64 friendUid);

    }
}
