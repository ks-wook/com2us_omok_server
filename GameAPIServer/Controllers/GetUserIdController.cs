using GameAPIServer.Controllers;
using GameAPIServer.Model.DTO;
using GameAPIServer.Repository;
using GameAPIServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace GameAPIServer.Controllers;

[Route("[controller]")]
[ApiController]
public class GetUserIdController : ControllerBase
{
    readonly ILogger<GetUserIdController> _logger;
    readonly IGameService _gameService;

    public GetUserIdController(ILogger<GetUserIdController> logger, IGameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    [HttpPost]
    public async Task<UserIdRes> GetUserId([FromBody] UserIdReq req)
    {
        UserIdRes res = new();

        (res.result, res.uid) = await _gameService.GetUserIdByAccountId(req.AccountId);
        
        return res;
    }
}
