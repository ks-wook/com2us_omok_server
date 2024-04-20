namespace GameAPIServer.Model.DTO
{
    public class FriendAddResReq
    {
        public Int64 Uid { get; set; }
        public Int64 FriendUid { get; set; }
        public bool IsAccept { get; set; }
    }


    public class FriendAddResRes
    {
        public ErrorCode Result { get; set; }
    }
}
