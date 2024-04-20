using GameAPIServer.Model.DTO;
using GameAPIServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameAPIServer.Controllers.Friend
{
    [Route("[controller]")]
    [ApiController]
    public class FriendAddResController : ControllerBase
    {
        readonly ILogger<FriendAddResController> _logger;
        readonly IFriendService _friendService;

        // 친구 요청에 대한 응답 처리
        public FriendAddResController(ILogger<FriendAddResController> logger, IFriendService friendService)
        {
            _logger = logger;
            _friendService = friendService;
        }


        [HttpPost]
        public async Task<FriendAddResRes> FriendAddRes([FromBody] FriendAddResReq req)
        {
            FriendAddResRes res = new FriendAddResRes();

            // Accept or Reject
            if(req.IsAccept)
            {
                res.Result = await _friendService.AcceptFriendReq(req.Uid, req.FriendUid);
            }
            else
            {
                res.Result = await _friendService.RejectFriendReq(req.Uid, req.FriendUid);
            }

            return res;
        }

    }
}
