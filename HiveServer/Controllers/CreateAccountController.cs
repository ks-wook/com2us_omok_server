using HiveServer;
using HiveServer.Services;
using HiveServer.Model.DAO;
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
    public async Task<CreateAccountRes> CreateAccount([FromBody]CreateAccountReq req)
    {
        CreateAccountRes res = new CreateAccountRes();

        if (!HiveServerSequrity.IsValidEmail(req.Email))
        {
            res.result = HiveServer.ErrorCode.InvalidEmailFormat;
            return res;
        }

        res.result = await _hiveDb.CreateAccountAsync(req.Email, req.Password);

        return res;
    }
}

