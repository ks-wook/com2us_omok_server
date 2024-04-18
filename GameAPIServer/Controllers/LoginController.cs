﻿using GameAPIServer.Model.DTO;
using GameAPIServer.Repository;
using GameAPIServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace GameAPIServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        readonly ILogger<LoginController> _logger;
        readonly IGameService _gameService;
        readonly IAuthenticationService _authenticationService;
        readonly IMemoryDb _memoryDb;

        public LoginController(ILogger<LoginController> logger, IGameService gameService, IAuthenticationService authenticationService, IMemoryDb memoryDb) 
        {
            _logger = logger;
            _gameService = gameService;
            _authenticationService = authenticationService;
            _memoryDb = memoryDb;

        }


        [HttpPost]
        public async Task<LoginRes> Login([FromBody] LoginReq req)
        {
            LoginRes res = new LoginRes();


            // 토큰을 hive서버로 보내서 유효성 검사
            res.Result = await _authenticationService.LoginTokenVerify(req.AccountId, req.Token);

            if(res.Result != ErrorCode.None) 
            {
                _logger.ZLogError(
                   $"[Login] UID = {req.AccountId}, TokenValidationCheckFail");
                return res;
            }


            // Game redis에 인증된 토큰을 삽입한다.
            res.Result = await _memoryDb.InsertGameLoginTokenAsync(req.AccountId, req.Token);
            if (res.Result != ErrorCode.None)
            {
                _logger.ZLogError(
                   $"[Login] UID = {req.AccountId}, Login Token Insert Fail");
                return res;
            }

            return res;
        }

    }
}