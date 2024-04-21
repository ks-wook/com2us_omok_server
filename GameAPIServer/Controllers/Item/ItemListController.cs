using GameAPIServer.Model.DTO;
using GameAPIServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameAPIServer.Controllers.Item
{
    [Route("[controller]")]
    [ApiController]
    public class ItemListController : ControllerBase
    {
        readonly ILogger<ItemListController> _logger;
        readonly IGameService _gameService;

        public ItemListController(ILogger<ItemListController> logger, IGameService gameService)
        {
            _logger = logger;
            _gameService = gameService;
        }

        [HttpPost]
        public async Task<ItemListRes> ItemList([FromBody] ItemListReq req)
        {
            ItemListRes res = new ItemListRes();

            (res.Result, res.ItemList) = await _gameService.GetItemListByUid(req.Uid);

            return res;
        } 
    }
}
