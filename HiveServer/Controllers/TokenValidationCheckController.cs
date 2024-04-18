using HiveServer.Model.DAO.HiveDb;
using HiveServer.Model.DAO.MemoryDb;
using HiveServer.Model.DTO;
using HiveServer.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace HiveServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TokenValidationCheckController : ControllerBase
    {
        readonly ILogger<TokenValidationCheckController> _logger;
        readonly IMemoryDb _memoryDb;

        public TokenValidationCheckController(ILogger<TokenValidationCheckController> logger, IMemoryDb memoryDb)
        {
            _logger = logger;
            _memoryDb = memoryDb;
        }


        [HttpPost]
        public async Task<TokenValidationCheckRes> TokenValidationCheck([FromBody] TokenValidationCheckReq req)
        {
            TokenValidationCheckRes res = new TokenValidationCheckRes();

            LoginToken? searchedToken;

            // hive redis에서 account 값을 이용해 토큰값 검색
            (res.Result, searchedToken) = await _memoryDb.GetHiveTokenByAccountId(req.AccountId);

            if(res.Result != ErrorCode.None || searchedToken == null) 
            {
                _logger.ZLogError
                    ($"[TokenValidationCheck] Uid:{req.AccountId}, Token:{req.Token} ErrorCode: {ErrorCode.TokenValidationCheckFail}");
                res.Result = ErrorCode.TokenValidationCheckFail;
                return res;
            }


            if(string.CompareOrdinal(req.Token, searchedToken.Token) != 0) // 토큰이 일치하지 않는 경우
            {
                _logger.ZLogError
                    ($"[TokenValidationCheck] Uid:{req.AccountId}, Token:{req.Token} ErrorCode: {ErrorCode.TokenMismatch}");
                res.Result = ErrorCode.TokenMismatch;
                return res;
            }

            return res;
        }


    }
}
