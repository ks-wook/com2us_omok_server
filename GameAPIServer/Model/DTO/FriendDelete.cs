namespace GameAPIServer.Model.DTO
{
    public class FriendDeleteReq
    {
        public Int64 Uid { get; set; }
        public Int64 FriendUId { get; set; }
    }

    public class FriendDeleteRes
    {
        public ErrorCode Result { get; set; }
    }
}
