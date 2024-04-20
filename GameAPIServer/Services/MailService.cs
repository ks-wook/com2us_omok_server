using GameAPIServer.Model.DAO.GameDb;
using GameAPIServer.Repository;
using ZLogger;

namespace GameAPIServer.Services
{
    public class MailService : IMailService
    {
        readonly ILogger<MailService> _logger;
        readonly IGameDb _gameDb;

        public MailService(ILogger<MailService> logger, IGameDb gameDb)
        {
            _logger = logger;
            _gameDb = gameDb;
        }


        public async Task<(ErrorCode, Mail?)> GetMailByMailId(Int64 mailId)
        {
            // 메일아이디로 메일 검색후 하나의 메일만 반환
            (ErrorCode result, dynamic? data) = await _gameDb.GetMailByMailId(mailId);
            
            if (result != ErrorCode.None || data == null)
            {
                return (result, null);
            }

            return (result, data);
        }

        public async Task<(ErrorCode, IEnumerable<Mail?>?)> GetMailListByUid(Int64 uid)
        {
            // Uid에 해당하는 메일리스트 전부 반환 
            (ErrorCode result, IEnumerable<Mail?>? data) = await _gameDb.GetMailListByUid(uid);

            if (result != ErrorCode.None || data == null)
            {
                return (result, null);
            }

            return (result, data);
        }

        public async Task<ErrorCode> ReceiveMailIReward(Int64 uid, Int64 mailId)
        {
            try
            {
                // 메일의 보상을 수신완료로 업데이트
                ErrorCode result = await _gameDb.UpdateMailRewardedBymailId(mailId);
                if(result != ErrorCode.None)
                {
                    _logger.ZLogError
                        ($"[ReceiveMailIReward] ErrorCode: {ErrorCode.FailReceiveMailReward}, mailId: {mailId}, uid: {uid}");
                    return ErrorCode.FailGetMail;
                }





                // 메일의 보상아이템들을 Item 테이블에 추가하여 유저에게 증정
                (result, IEnumerable<MailItem?>? mailItemList) = await _gameDb.GetMailItemListByMailId(mailId);
                if(result != ErrorCode.None || mailItemList == null)
                {
                    _logger.ZLogError
                        ($"[ErrReceiveMailIRewardorCode] ErrorCode: {ErrorCode.FailReceiveMailReward}, mailId: {mailId}, uid: {uid}");
                    return ErrorCode.FailGetMail;
                }

                result = await _gameDb.CreateItemListByMailItemList(uid, mailItemList);
                if(result != ErrorCode.None ) 
                {
                    _logger.ZLogError
                        ($"[ErrReceiveMailIRewardorCode] ErrorCode: {ErrorCode.FailReceiveMailReward}, mailId: {mailId}, uid: {uid}");
                    return ErrorCode.FailGetMail;
                }




                return result;
            
            }
            catch
            {
                _logger.ZLogError
                        ($"[ErrReceiveMailIRewardorCode] ErrorCode: {ErrorCode.FailReceiveMailReward}, mailId: {mailId}, uid: {uid}");
                return ErrorCode.FailGetMail;
            }
        }
    }
}
