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
    public string TestAPI([FromHeader] string message)
    {
        Console.WriteLine(message);
        return "hello client";
    }
}

