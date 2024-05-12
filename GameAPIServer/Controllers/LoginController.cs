using GameAPIServer.Model.DAO.GameDb;
using GameAPIServer.Model.DTO;
using GameAPIServer.Repository;
using GameAPIServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using ZLogger;

namespace GameAPIServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        readonly ILogger<LoginController> _logger;
        readonly IGameService _gameService;
        readonly IAuthenticationService _authenticationService;
        readonly IMemoryDb _memoryDb;

        public LoginController(ILogger<LoginController> logger, IGameService gameService, IAuthenticationService authenticationService, IMemoryDb memoryDb) 
        {
            _logger = logger;
            _gameService = gameService;
            _authenticationService = authenticationService;
            _memoryDb = memoryDb;

        }

        // 유저 정보가 있다면 같이 동봉해서 보내고, 없다면 신규 유저이므로 새로 만들어서 전달
        [HttpPost]
        public async Task<LoginRes> Login([FromBody] LoginReq req)
        {
            LoginRes res = new LoginRes();

            res.Result = await _authenticationService.Login(req.AccountId, req.Token);

            if(res.Result != ErrorCode.None)
            {
                _logger.ZLogError
                    ($"[Login] ErrorCode: {ErrorCode.FailLogin}, accountId: {req.AccountId}");
                return res;
            }

            // 로그인 성공 시 게임 데이터 로드
            res.UserGameData = await _gameService.LoadGameData(req.AccountId);

            return res;
        }

    }
}
