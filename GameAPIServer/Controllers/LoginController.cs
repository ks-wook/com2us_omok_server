using GameAPIServer.Model.DTO;
using GameAPIServer.Repository;
using GameAPIServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameAPIServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        readonly ILogger<LoginController> _logger;
        readonly IGameService _gameService;
        readonly IMemoryDb _memoryDb;

        public LoginController(ILogger<LoginController> logger, IGameService gameService, IMemoryDb memoryDb) 
        {
            _logger = logger;
            _gameService = gameService;
            _memoryDb = memoryDb;

        }


        [HttpPost]
        public async Task<LoginRes> Login([FromBody] LoginReq req)
        {
            LoginRes res = new LoginRes();


            // TODO 토큰을 hive서버로 보내서 유효성 검사
            



            // TODO 유효성 검사 통과 시 로그인 성공 메시지를 전송하고




            // TODO Game redis에 인증된 토큰을 삽입한다.
            



            return res;
        }

    }
}
