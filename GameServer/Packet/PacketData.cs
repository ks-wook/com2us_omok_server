using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

[MemoryPackable]
public partial class PkResult : PkHeader // 결과가 포함된 패킷
{
    public ErrorCode Result = ErrorCode.None;
}

[MemoryPackable]
public partial class PKTReqLogin : PkHeader // 게임 서버 로그인 요청
{
    public string UserId { get; set; } = string.Empty; // 유저 아이디 = 계정 아이디
    public string AuthToken { get; set; } = string.Empty;
}

[MemoryPackable]
public partial class PKTResLogin : PkResult // 로그인 요청 응답
{
    public string UserId { get; set; } = string.Empty;
}

[MemoryPackable]
public partial class PKTReqRoomEnter : PkHeader // 방 입장 요청
{
    public int RoomNumber = 0; // 0번방은 존재 하지 않으므로 0이라면 잘못된 패킷
}

[MemoryPackable]
public partial class PKTResRoomEnter : PkResult // 방 입장 요청 응답
{
    public string UserId { get; set; } = string.Empty; // 입장에 성공한 UserId
    public Int32 RoomNumber { get; set; } // 입장에 성공한 방 number
}

[MemoryPackable]
public partial class PKTReqRoomLeave : PkHeader // 방 퇴장 요청
{
}

[MemoryPackable]
public partial class PKTResRoomLeave : PkResult // 방 퇴장 요청 응답
{
    public String UserId { get; set; } = string.Empty; // 퇴장한 유저의 user id
}

[MemoryPackable]
public partial class PKTReqRoomChat : PkHeader // 방 채팅 요청
{
    public string ChatMsg { get; set; } = string.Empty;
}

[MemoryPackable]
public partial class PKTResRoomChat : PkResult // 방 채팅 요청 응답
{
    public string UserId { get; set; } = string.Empty;
    public string ChatMsg { get; set; } = string.Empty;
}

// 오목 로직 패킷
[MemoryPackable]
public partial class PKTReqReadyOmok : PkHeader // 오목 플레이 준비 완료 요청
{
}

[MemoryPackable]
public partial class PKTResReadyOmok : PkResult // 오목 플레이 준비 완료 요청 응답
{
    public string UserId { get; set; } = string.Empty; // 준비 상태가 변경된 유저의 ID
    public bool IsReady { get; set; } // 변경된 준비 완료 여부
}

[MemoryPackable]
public partial class PKTNtfStartOmok : PkHeader // 게임 시작 통보 패킷
{
    public string BlackUserId { get; set; } = string.Empty; // 선공인 유저 ID
    public string WhiteUserId { get; set; } = string.Empty; // 선공인 유저 ID
}

[MemoryPackable]
public partial class PKTReqPutMok : PkHeader // 돌 두기 요청
{
    public int PosX;
    public int PosY;
}

[MemoryPackable]
public partial class PKTResPutMok : PkResult // 돌 두기 요청 응답
{
    public string UserId { get; set; } = string.Empty;
    public int PosX;
    public int PosY;
    public bool IsBlack { get; set; } = false;
    public bool IsTimeout { get; set; } = false;

}

[MemoryPackable]
public partial class PKTNtfEndOmok : PkResult // 게임 종료 통보 패킷
{
    public string WinUserId { get; set; } = string.Empty; // 승리한 유저 ID
}


[MemoryPackable]
public partial class PKTReqPing : PkHeader // 유저 상태 검사
{
}


[MemoryPackable]
public partial class PKTResPing : PkResult // 유저 상태 검사 응답
{
}