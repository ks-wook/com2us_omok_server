using GameServer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmokClient.Game;

public class LoginReq
{
    [Required]
    public Int64 AccountId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;
}

public class LoginRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public UserGameData? UserGameData { get; set; }
}

public class UserGameData
{
    public Int64 uid { get; set; }
    public Int64 account_id { get; set; }
    public string nickname { get; set; } = string.Empty;
    public int user_money { get; set; } = 0;
    public string created_at { get; set; } = string.Empty;
    public string recent_login_at { get; set; } = string.Empty;
    public int user_level { get; set; } = 0;
    public int user_exp { get; set; } = 0;
    public int total_win_cnt { get; set; } = 0;
    public int total_lose_cnt { get; set; } = 0;
}

public class MatchingRequest
{
    public string UserID { get; set; }
}

public class MatchResponse
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
}

public class CheckMatchingReq
{
    public string UserID { get; set; } = "";
}

public class MatchingCompleteData
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public string UserID { get; set; } = string.Empty;
    public bool IsMatched { get; set; } = false;
    public string ServerAddress { get; set; } = "";
    public int Port { get; set; }
    public int RoomNumber { get; set; } = 0;
}