using GameAPIServer.Model.DTO;
using GameAPIServer.Repository;
using ZLogger;

namespace GameAPIServer.Services;

public class AuthenticationService : IAuthenticationService
{
    readonly ILogger<AuthenticationService> _logger;
    readonly IGameDb _gameDb;
    readonly IMemoryDb _memoryDb;
    string _tokenValidationCheckAPI;

    public AuthenticationService(ILogger<AuthenticationService> logger, IConfiguration configuration, IGameDb gameDb, IMemoryDb memoryDb)
    {
        _gameDb = gameDb;
        _logger = logger;
        _tokenValidationCheckAPI = configuration.GetSection("HiveServerAddr").Value + "/tokenValidationCheck";
        _memoryDb = memoryDb;
    }



    // Hive 서버로 로그인한 계정의 토큰 유효성 검증을 요청
    public async Task<ErrorCode> LoginTokenVerify(Int64 accuountId, string loginToken)
    {
        try
        {
            HttpClient client = new HttpClient();
            TokenValidationCheckReq req = new TokenValidationCheckReq()
            {
                AccountId = accuountId,
                Token = loginToken
            };

            HttpResponseMessage httpRes = await client.PostAsJsonAsync(_tokenValidationCheckAPI, req);
                
            if(httpRes == null || httpRes.StatusCode != System.Net.HttpStatusCode.OK) 
            {
                _logger.ZLogDebug($"[LoginTokenVerify] ErrorCode:{ErrorCode.InvalidResFromHive}, " +
                    $"AccountId = {accuountId}, Token = {loginToken}, StatusCode = {httpRes?.StatusCode}");
                return ErrorCode.InvalidResFromHive;
            }


            TokenValidationCheckRes res = await httpRes.Content.ReadFromJsonAsync<TokenValidationCheckRes>()
                ?? new TokenValidationCheckRes{ Result = ErrorCode.InvalidResFromHive };
            

            if(res.Result != ErrorCode.None)
            {




                return ErrorCode.InvalidResFromHive;
            }


            return ErrorCode.None;

        }
        catch(Exception e) 
        {
            _logger.ZLogDebug($"[LoginTokenVerify] " +
                $"ErrorCode:{ErrorCode.InvalidResFromHive}, AccountId = {accuountId}, Token = {loginToken}");
            return ErrorCode.InvalidResFromHive;
        }
    }


}
