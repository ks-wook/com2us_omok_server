using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Packet;


[MemoryPackable]
public partial class PKTInnerReqSaveGameResult : PkHeader // 게임 결과 저장 요청
{
    public List<string> sessionIds = new List<string>();
    public string BlackUserId { get; set; } = string.Empty;
    public string WhiteUserId { get; set; } = string.Empty;
    public string WinUserId { get; set; } = string.Empty;

}

[MemoryPackable]
public partial class PKTInnerResSaveGameResult : PkResult // 게임 결과 저장 요청 응답
{
    public List<string> sessionIds = new List<string>();
    public string WinUserId { get; set; } = string.Empty;
}

[MemoryPackable]
public partial class PKTInnerReqVerifyToken : PkHeader // 로그인 토큰 검증 요청
{
    public Int64 AccountId { get; set; } // redis에 저장되는 토큰은 userId가 아닌 accountId를 key로 저장
    public string Token { get; set; } = string.Empty;
}

[MemoryPackable]
public partial class PKTInnerResVerifyToken : PkResult // 로그인 토큰 검증 요청 응답
{
    public string UserId { get; set; } = string.Empty;
}

[MemoryPackable]
public partial class PKTInnerNtfTurnChange : PkHeader // 유저의 턴을 강제로 넘기라는 요청
{
    public string CurTurnUserId {  get; set; } = string.Empty;
    public int RoomNumber { get; set; }
}

[MemoryPackable]
public partial class PKTInnerNtfSendPing : PkHeader // 다시 핑을 보내라는 Inner packet
{
    public string SessionId { get; set; } = string.Empty;
}

[MemoryPackable]
public partial class PKTInnerNtfCloseConnection : PkHeader // 핑이 제때에 오지못한 유저 disconnect
{
    public string SessionId { get; set; } = string.Empty;
}