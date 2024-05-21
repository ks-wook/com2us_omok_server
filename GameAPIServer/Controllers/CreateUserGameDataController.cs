using GameAPIServer.Model.DAO.GameDb;
using GameAPIServer.Model.DTO;
using GameAPIServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameAPIServer.Controllers;

[Route("[controller]")]
[ApiController]
public class CreateUserGameDataController : ControllerBase
{
    readonly ILogger<CreateUserGameDataController> _logger;
    readonly IGameService _gameService;

    public CreateUserGameDataController(ILogger<CreateUserGameDataController> logger, IGameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    [HttpPost]
    public async Task<CreateUserGameDataRes> Post([FromBody] CreateUserGameDataReq req)
    {
        CreateUserGameDataRes res = new();

        (res.result, _) = await _gameService.InitNewUserGameData(req.AccountId);

        return res;
    }
}
