using GameAPIServer.Model.DAO.GameDb;
using GameAPIServer.Model.DTO;
using GameAPIServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace GameAPIServer.Controllers.Friend
{
    [Route("[controller]")]
    [ApiController]
    public class FriendAddReqController : ControllerBase
    {
        readonly ILogger<FriendAddReqController> _logger;
        readonly IFriendService _friendService;

        public FriendAddReqController(ILogger<FriendAddReqController> logger, IFriendService friendService)
        {
            _logger = logger;
            _friendService = friendService;
        }

        [HttpPost]
        public async Task<FriendAddReqRes> FriendAddReq ([FromBody] FriendAddReqReq req)
        {
            // TODO 친구 요청에 대해 처리
            FriendAddReqRes res = new FriendAddReqRes();








            throw new NotImplementedException();
        }

    }
}
