using HiveServer.Model.DTO;
using HiveServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;


[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    readonly string? _tokenSaltValue;
    readonly ILogger<LoginController> _logger;
    readonly IHiveDb _hiveDb;

    public LoginController(ILogger<LoginController> logger, IHiveDb hiveDb, IConfiguration config)
    {
        _logger = logger;
        _hiveDb = hiveDb;

        _tokenSaltValue = config.GetSection("TokenSaltValue").Value;
        if(_tokenSaltValue == null ) 
        {
            _logger.ZLogError(
                $"[Login] Null TokenSaltValue");
        }

    }


    [HttpPost]
    public async Task<LoginRes> Login([FromHeader] LoginReq req)
    {
        LoginRes res = new LoginRes();

        
        // 전달된 패스워드를 이용해 해싱값을 만들고 일치하는지 검사
        res.result = await _hiveDb.VerifyUserAsync(req.Email, req.Password);
        if(res.result == HiveServer.ErrorCode.None ) // 로그인 성공 시 토큰 발급
        { 
            // TODO 토큰 발급






            // TODO 레디스에 토큰 저장







        }


        return res;
    }
}

