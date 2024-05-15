using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace GameAPIServer.Controllers;

[ApiController]
[Route("[controller]")]
public class CheckMatchingController : Controller
{
    readonly ILogger<RequestMatchingController> _logger;
    string _checkMatchAPI;

    public CheckMatchingController(ILogger<RequestMatchingController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _checkMatchAPI = configuration.GetSection("MatchAPIServer").Value + "/CheckMatching";
    }

    [HttpPost]
    public async Task<CheckMatchingRes> Post(CheckMatchingReq request)
    {
        // match api 서버로 요청을 보내서 매칭이 완료되었는지 묻는다.
        CheckMatchingRes res = await RequestCheckMatchingComplete(request);

        return res;
    }

    public async Task<CheckMatchingRes> RequestCheckMatchingComplete(CheckMatchingReq request)
    {
        HttpClient client = new();

        HttpResponseMessage httpRes = await client.PostAsJsonAsync<CheckMatchingReq>(_checkMatchAPI, request);
        
        if (httpRes == null || httpRes.StatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.ZLogError($"[RequestCheckMatchingComplete] ErrorCode:{ErrorCode.FailCheckMatch}, " +
                $"userId = {request.UserID}, StatusCode = {httpRes?.StatusCode}");
            return new CheckMatchingRes { Result = ErrorCode.FailCheckMatch };
        }

        _logger.ZLogDebug($"[RequestCheckMatchingComplete] 매칭 서버로부터 매칭 완료 정보 수신");

        CheckMatchingRes res = await httpRes.Content.ReadFromJsonAsync<CheckMatchingRes>() 
            ?? new CheckMatchingRes { Result = ErrorCode.FailCheckMatch };
        
        return res;
    }
}

public class CheckMatchingReq
{
    public string UserID { get; set; }
}

public class CheckMatchingRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public string UserID { get; set; } = string.Empty;
    public bool IsMatched { get; set; } = false;
    public string ServerAddress { get; set; } = "";
    public int Port { get; set; }
    public int RoomNumber { get; set; } = 0;
}