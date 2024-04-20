namespace GameAPIServer.Model.DTO
{
    // 친구추가 요청
    public class FriendAddReqReq
    {
        public Int64 Uid {  get; set; }
        public Int64 FriendUid { get; set; }
    }

    // 친구추가 요청에 대한 결과
    public class FriendAddReqRes
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
    }
}
