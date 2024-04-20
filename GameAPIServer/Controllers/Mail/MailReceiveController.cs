using GameAPIServer.Model.DTO;
using GameAPIServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameAPIServer.Controllers.Mailbox
{
    [Route("[controller]")]
    [ApiController]
    public class MailReceiveController : ControllerBase
    {
        readonly ILogger<MailReceiveController> _logger;
        readonly IMailService _mailService;

        public MailReceiveController(ILogger<MailReceiveController> logger, IMailService mailService)
        {
            _logger = logger;
            _mailService = mailService;
        }

        [HttpPost]
        public async Task<MailReceiveRes> MailReceive([FromBody] MailReceiveReq req)
        {
            MailReceiveRes res = new MailReceiveRes();

            res.Result = await _mailService.ReceiveMailIReward(req.Uid, req.MailId);

            return res;
        }
    }
}
