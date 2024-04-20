using GameAPIServer.Model.DTO;
using GameAPIServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameAPIServer.Controllers.Friend
{
    [Route("[controller]")]
    [ApiController]
    public class FriendDeleteController : ControllerBase
    {
        readonly ILogger<FriendDeleteController> _logger;
        readonly IFriendService _friendService;

        // 친구 요청에 대한 응답 처리
        public FriendDeleteController(ILogger<FriendDeleteController> logger, IFriendService friendService)
        {
            _logger = logger;
            _friendService = friendService;
        }

        [HttpPost]
        public async Task<FriendDeleteRes> FriendDelete([FromBody] FriendDeleteReq req)
        {
            FriendDeleteRes res = new FriendDeleteRes();

            res.Result = await _friendService.DeleteFriend(req.Uid, req.FriendUId);
            
            return res;
        }
    }
}
