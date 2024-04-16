using APIAccountServer;
using APIAccountServer.Services;
using HiveServer.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("[controller]")]
public class CreateAccountController : ControllerBase
{
    readonly ILogger<CreateAccountController> _logger;
    readonly IHiveDb _hiveDb;

    public CreateAccountController(ILogger<CreateAccountController> logger, IHiveDb hivedb)
    {
        _logger = logger;
        _hiveDb = hivedb;
    }

    [HttpPost]
    public async Task<CreateAccountRes> CreateAccount([FromBody] CreateAccountReq req)
    {
        CreateAccountRes res = new CreateAccountRes();

        if(!HiveServerSequrity.IsValidEmail(req.email))
        {
            res.result = HiveServer.ErrorCode.InvalidEmailFormat;
        }

        // DB 최소 저장 데이터 이메일, 패스워드
        res.result = await _hiveDb.CreateAccountAsync(req.email, req.password);

        return res;
    }


}

