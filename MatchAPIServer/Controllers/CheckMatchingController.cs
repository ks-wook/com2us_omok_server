using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZLogger;
using static APIServer.Controllers.CheckMatching;

namespace APIServer.Controllers;

[ApiController]
[Route("[controller]")]
public class CheckMatching : Controller
{
    IMatchWoker _matchWorker;

    public CheckMatching(IMatchWoker matchWorker)
    {
        _matchWorker = matchWorker;
    }

    [HttpPost]
    public CheckMatchingRes Post(CheckMatchingReq request)
    {
        CheckMatchingRes response = new();

        (var result, var completeMatchingData) = _matchWorker.GetCompleteMatching(request.UserID);

        if(result == false || completeMatchingData == null)
        {
            response.IsMatched = false;
        }
        else
        {
            response.IsMatched = true;
            response.ServerAddress = completeMatchingData.ServerAddress;
            response.RoomNumber = completeMatchingData.RoomNumber;
        }

        return response;
    }
}

public class CheckMatchingReq
{
    public string UserID { get; set; } = "";
}

public class CheckMatchingRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public bool IsMatched { get; set; } = false;
    public string ServerAddress { get; set; } = "";
    public int RoomNumber { get; set; } = 0;
}