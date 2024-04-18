using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameAPIServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LogoutController : ControllerBase
    {
        public LogoutController() { }

        // TODO 로그아웃 메서드 구현
        

        // TODO 사용자 토큰을 hive redis에서 제거하라는 요청 + 
        // TODO hive redis에서 제거 완료했다는 요청이 들어오면 game redis에서도 삭제



    }
}
