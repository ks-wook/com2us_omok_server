using HiveServer;
using HiveServer.Model.DTO;
using HiveServer.Repository;
using HiveServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;


[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    readonly string _tokenSaltValue;
    readonly ILogger<LoginController> _logger;
    readonly IHiveDb _hiveDb;
    readonly IMemoryDb _memoryDb;

    public LoginController(ILogger<LoginController> logger, IHiveDb hiveDb, IMemoryDb memoryDb, IConfiguration config)
    {
        _logger = logger;
        _hiveDb = hiveDb;
        _memoryDb = memoryDb;

        _tokenSaltValue = config.GetSection("TokenSaltValue").Value ?? "error";
    }


    [HttpPost]
    public async Task<LoginRes> Login([FromBody] LoginReq req)
    {
        LoginRes res = new LoginRes();

        
        // 전달된 패스워드를 이용해 해싱값을 만들고 일치하는지 검사
        (res.Result, res.AccountId) = await _hiveDb.VerifyUserAsync(req.Email, req.Password);
        if(res.Result == HiveServer.ErrorCode.None) // 로그인 성공 시 토큰 발급
        {
            if(_tokenSaltValue == "error") 
            {
                res.Result = ErrorCode.NullServerToken;
                return res;
            }

            res.LoginToken = HiveServerSequrity.GenerateLoginToken(res.AccountId, _tokenSaltValue);

            // hive redis에 토큰 저장
            res.Result = await _memoryDb.InsertHiveLoginTokenAsync(res.AccountId, res.LoginToken);

        }

        return res;
    }
}

