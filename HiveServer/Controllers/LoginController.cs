using HiveServer;
using HiveServer.Model.DTO;
using HiveServer.Repository;
using HiveServer.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ZLogger;


[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    readonly string _tokenSaltValue;
    readonly ILogger<LoginController> _logger;
    readonly IHiveDb _hiveDb;
    readonly IMemoryDb _memoryDb;

    string _getUserIdAPI;

    public LoginController(ILogger<LoginController> logger, IHiveDb hiveDb, IMemoryDb memoryDb, IConfiguration configuration)
    {
        _logger = logger;
        _hiveDb = hiveDb;
        _memoryDb = memoryDb;
        
        _tokenSaltValue = configuration.GetSection("TokenSaltValue").Value ?? "error";
        _getUserIdAPI = configuration.GetSection("GameAPIServerAddr").Value + "/GetUserId";
    }

    [HttpPost]
    public async Task<LoginRes> Login([FromBody] LoginReq req)
    {
        LoginRes res = new LoginRes();

        if (_tokenSaltValue == "error")
        {
            _logger.ZLogError
            ($"[Login] ErrorCode: {ErrorCode.NullServerToken}");
            res.Result = ErrorCode.NullServerToken;
            return res;
        }

        // 전달된 패스워드를 이용해 해싱값을 만들고 일치하는지 검사
        (res.Result, Int64 accountId) = await _hiveDb.VerifyUserAndGetAccountIdAsync(req.Id, req.Password);

        if(res.Result != ErrorCode.None)
        {
            _logger.ZLogError
                ($"[Login] ErrorCode: {ErrorCode.FailVerifyUser}");
            res.Result = ErrorCode.FailVerifyUser;
            return res;
        }

        // account id 값을 이용해서 user id 값을 획득
        (bool result, Int64 uid) = await GetUserIdByAccountIdReq(accountId);
        if(result == false)
        {
            _logger.ZLogError
                ($"[Login] ErrorCode: {ErrorCode.FailVerifyUser}");
            res.Result = ErrorCode.FailVerifyUser;
            return res;
        }

        res.UserId = uid;

        // 획득한 uid를 통해 토큰 발급
        res.Token = HiveServerSequrity.GenerateLoginToken(res.UserId, _tokenSaltValue);

        // hive redis에 토큰 저장
        res.Result = await _memoryDb.InsertHiveLoginTokenAsync(res.UserId, res.Token);

        return res;
    }

    // Http 요청을 보내서 uid 획득
    async Task<(bool, Int64)> GetUserIdByAccountIdReq(Int64 accountId)
    {
        HttpClient client = new HttpClient();
        UserIdReq userIdreq = new UserIdReq()
        {
            AccountId = accountId,
        };

        HttpResponseMessage httpRes = await client.PostAsJsonAsync<UserIdReq>(_getUserIdAPI, userIdreq);
        await Console.Out.WriteLineAsync(_getUserIdAPI);

        if (httpRes == null || httpRes.StatusCode != System.Net.HttpStatusCode.OK)
        {

            _logger.ZLogError
                ($"[GetUserIdByAccountIdReq] ErrorCode: {ErrorCode.FailGetUidByAccountId}");
            return (false, -1);
        }
        UserIdRes? userIdRes = await httpRes.Content.ReadFromJsonAsync<UserIdRes>();
        if(userIdRes == null || userIdRes.result != ErrorCode.None)
        {
            _logger.ZLogError
                ($"[GetUserIdByAccountIdReq] ErrorCode: {ErrorCode.FailGetUidByAccountId}");
            return (false, -1);
        }

        return (true, userIdRes.uid);
    }
}

