using GameAPIServer.Model.DAO.GameDb;

namespace GameAPIServer.Model.DTO
{
    public class MailListReq
    {
        public Int64 Uid { get; set; }
    }


    public class MailListRes
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
        public IEnumerable<Mail?>? mailList { get; set; }
    }
}
