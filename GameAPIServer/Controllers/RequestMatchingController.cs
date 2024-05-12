using GameAPIServer.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ZLogger;

namespace GameAPIServer.Controllers;

[ApiController]
[Route("[controller]")]
public class RequestMatchingController : ControllerBase
{
    readonly ILogger<RequestMatchingController> _logger;
    string _requestMatchAPI;

    public RequestMatchingController(ILogger<RequestMatchingController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _requestMatchAPI = configuration.GetSection("MatchAPIServer").Value + "/RequestMatching";
    }

    [HttpPost]
    public async Task<MatchResponse> Post(MatchingRequest request)
    {
        MatchResponse response = new();

        ErrorCode result = await RequestMatching(request);
        
        return response;
    }

    public async Task<ErrorCode> RequestMatching(MatchingRequest request)
    {
        // matching api 서버로 매칭 요청 전송
        HttpClient client = new HttpClient();
        
        HttpResponseMessage httpRes = await client.PostAsJsonAsync<MatchingRequest>(_requestMatchAPI, request);

        if (httpRes == null || httpRes.StatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.ZLogDebug($"[RequestMatching] ErrorCode: {ErrorCode.FailRequestMatch}, " +
                $"userId = {request.UserID}, StatusCode = {httpRes?.StatusCode}");
            return ErrorCode.FailRequestMatch;
        }

        return ErrorCode.None;
    }
}

public class MatchingRequest
{
    public string UserID { get; set; }
}

public class MatchResponse
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
}