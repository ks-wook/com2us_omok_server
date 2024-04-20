using GameAPIServer.Model.DTO;
using GameAPIServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameAPIServer.Controllers.Mailbox
{
    [Route("[controller]")]
    [ApiController]
    public class MailListController : ControllerBase
    {
        readonly ILogger<MailListController> _logger;
        readonly IMailService _mailService;

        public MailListController(ILogger<MailListController> logger, IMailService mailService)
        {
            _logger = logger;
            _mailService = mailService;
        }

        [HttpPost]
        public async Task<MailListRes> MailList([FromBody] MailListReq req)
        {
            MailListRes res = new MailListRes();

            (res.Result, res.mailList) = await _mailService.GetMailListByUid(req.Uid);

            return res;
        }

    }
}
