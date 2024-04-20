namespace GameAPIServer.Model.DTO
{
    public class MailReceiveReq
    {
        public Int64 Uid { get; set; }
        public Int64 MailId {  get; set; }
    }

    public class MailReceiveRes
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
    }
}
