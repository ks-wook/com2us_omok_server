using GameAPIServer.Model.DAO.GameDb;
using GameAPIServer.Model.DTO;
using GameAPIServer.Repository;
using ZLogger;

namespace GameAPIServer.Services;

public class AuthenticationService : IAuthenticationService
{
    readonly ILogger<AuthenticationService> _logger;
    readonly IGameDb _gameDb;
    readonly IMemoryDb _memoryDb;
    readonly IGameService _gameService;
    string _tokenValidationCheckAPI;

    public AuthenticationService(ILogger<AuthenticationService> logger, IConfiguration configuration, IGameDb gameDb, IMemoryDb memoryDb, IGameService gameService)
    {
        _gameDb = gameDb;
        _logger = logger;
        _tokenValidationCheckAPI = configuration.GetSection("HiveServerAddr").Value + "/tokenValidationCheck";
        _memoryDb = memoryDb;
        _gameService = gameService;
    }

    // 게임 API 서버 로그인 처리
    public async Task<ErrorCode> Login(Int64 accountId, string loginToken)
    {
        // 토큰을 hive서버로 보내서 유효성 검사
        ErrorCode result = await LoginTokenVerify(accountId, loginToken);

        if (result != ErrorCode.None)
        {
            _logger.ZLogError(
               $"[Login] UID = {accountId}, TokenValidationCheckFail");
            return result;
        }

        // Game redis에 인증된 토큰을 삽입한다.
        result = await _memoryDb.InsertGameLoginTokenAsync(accountId, loginToken);
        if (result != ErrorCode.None)
        {
            _logger.ZLogError(
               $"[Login] UID = {accountId}, Login Token Insert Fail");
            return result;
        }

        return result;
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
                _logger.ZLogInformation
                    ($"[LoginTokenVerify] ErrorCode:{ErrorCode.InvalidResFromHive}, AccountId = {accuountId}, Token = {loginToken}");
                return ErrorCode.InvalidResFromHive;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError($"[LoginTokenVerify] " +
                $"ErrorCode:{ErrorCode.InvalidResFromHive}, AccountId = {accuountId}, Token = {loginToken}");
            return ErrorCode.InvalidResFromHive;
        }
    }
}
