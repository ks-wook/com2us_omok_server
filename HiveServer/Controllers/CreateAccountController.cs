using HiveServer;
using HiveServer.Services;
using HiveServer.Model.DAO;
using HiveServer.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;


[ApiController]
[Route("[controller]")]
public class CreateAccountController : ControllerBase
{
    readonly ILogger<CreateAccountController> _logger;
    readonly IHiveDb _hiveDb;

    string _createUserGameDataAPI;

    public CreateAccountController(ILogger<CreateAccountController> logger, IHiveDb hivedb, IConfiguration configuration)
    {
        _logger = logger;
        _hiveDb = hivedb;

        _createUserGameDataAPI = configuration.GetSection("GameAPIServerAddr").Value + "/CreateUserGameData";
    }

    [HttpPost]
    public async Task<CreateAccountRes> CreateAccount([FromBody]CreateAccountReq req)
    {
        CreateAccountRes res = new CreateAccountRes();

        if (!HiveServerSequrity.IsValidEmail(req.Id))
        {
            res.result = HiveServer.ErrorCode.InvalidEmailFormat;
            return res;
        }

        (res.result, Int64 accountId) = await _hiveDb.CreateAccountAsync(req.Id, req.Password);

        // game api 서버로 UserGameData 생성 요청
        res.result = await CreateUserGameDataReq(accountId);

        return res;
    }

    async Task<ErrorCode> CreateUserGameDataReq(Int64 accountId)
    {
        HttpClient client = new HttpClient();
        CreateUserGameDataReq createUserGameDataReq = new CreateUserGameDataReq()
        {
            AccountId = accountId,
        };

        HttpResponseMessage httpRes = await client.PostAsJsonAsync<CreateUserGameDataReq>(_createUserGameDataAPI, createUserGameDataReq);
        if (httpRes == null || httpRes.StatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.ZLogError
                ($"[CreateUserGameDataReq] ErrorCode: {ErrorCode.FailCreateUserGameData}");
            return ErrorCode.FailCreateUserGameData;
        }

        CreateUserGameDataRes? res = await httpRes.Content.ReadFromJsonAsync<CreateUserGameDataRes>();
        if(res == null || res.result != ErrorCode.None)
        {
            _logger.ZLogError
                ($"[CreateUserGameDataReq] ErrorCode: {ErrorCode.FailCreateUserGameData}");
            return ErrorCode.FailCreateUserGameData;
        }

        return ErrorCode.None;
    }

}

