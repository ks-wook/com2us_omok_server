using GameAPIServer.Model.DAO.GameDb;

namespace GameAPIServer.Services
{
    public class MailService : IMailService
    {
        readonly ILogger<MailService> _logger;
        readonly IGameService _gameService;

        public MailService(ILogger<MailService> logger, IGameService gameService)
        {
            _logger = logger;
            _gameService = gameService;
        }


        public Task<(ErrorCode, Mail?)> GetMailByMailId(Int64 mailId)
        {
            // TODO 메일아이디로 메일 검색후 하나의 메일만 반환








            throw new NotImplementedException();
        }

        public Task<(ErrorCode, IEnumerable<Mail?>?)> GetMailListByUid(Int64 uid)
        {
            // Uid에 해당하는 메일리스트 전부 반환







            throw new NotImplementedException();
        }

        public Task<ErrorCode> ReceiveMailIReward(Int64 uid, Int64 mailId)
        {
            // TODO 메일의 보상을 수신완료로 업데이트







            // TODO 메일의 보상아이템들을 Item 테이블에 추가하여 유저에게 증정










            throw new NotImplementedException();
        }
    }
}
