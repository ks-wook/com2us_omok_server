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


            // 토큰을 hive서버로 보내서 유효성 검사
            res.Result = await _authenticationService.LoginTokenVerify(req.AccountId, req.Token);

            if(res.Result != ErrorCode.None) 
            {
                _logger.ZLogError(
                   $"[Login] UID = {req.AccountId}, TokenValidationCheckFail");
                return res;
            }


            // Game redis에 인증된 토큰을 삽입한다.
            res.Result = await _memoryDb.InsertGameLoginTokenAsync(req.AccountId, req.Token);
            if (res.Result != ErrorCode.None)
            {
                _logger.ZLogError(
                   $"[Login] UID = {req.AccountId}, Login Token Insert Fail");
                return res;
            }
            
            //  게임 데이터를 찾고 res에 담아 반환
            UserGameData? userGameData = await LoadGameData(req.AccountId);
            res.UserGameData = userGameData;


            return res;
        }


        public async Task<UserGameData?> LoadGameData(Int64 accountId)
        {
            // accountId 값을 사용하여 게임 데이터를 검색
            (ErrorCode result, UserGameData? userGameData) = await _gameService.GetGameDataByAccountId(accountId);
            
            if(userGameData == null)
            {
                // UserGameData가 존재하지 않는 경우 새로운 유저 게임 데이터를 생성하여 반환
                (result, userGameData) = await _gameService.InitNewUserGameData(accountId);
            }

            return userGameData;
        }

    }
}
