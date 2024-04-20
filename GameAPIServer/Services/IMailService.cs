using GameAPIServer.Model.DAO.GameDb;

namespace GameAPIServer.Services
{
    public interface IMailService
    {
        public Task<(ErrorCode, Mail?)> GetMailByMailId(Int64 mailId);

        public Task<(ErrorCode, IEnumerable<Mail?>?)> GetMailListByUid(Int64 uid);


        public Task<ErrorCode> ReceiveMailIReward(Int64 uid, Int64 mailId);
    }
}
