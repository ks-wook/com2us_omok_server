using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Packet;

[MemoryPackable]
public partial class S_Test : PkHeader // 테스트 요청 응답
{
    public string Msg { get; set; } = string.Empty;
}



[MemoryPackable]
public partial class S_LoginReq : PkHeader // 로그인 요청 응답
{
    public ErrorCode Result { get; set; }
    public string UserId { get; set; } = string.Empty;
}


[MemoryPackable]
public partial class S_LogoutReq : PkHeader // 게임 서버 로그아웃 요청 응답
{
    public ErrorCode Result { get; set; }
}

[MemoryPackable]
public partial class S_EnterRoomReq : PkHeader // 방 입장 요청 응답
{
    public ErrorCode Result { get; set; }
    public string UserId { get; set; } = string.Empty; // 입장에 성공한 UserId
    public Int32 RoomNumber { get; set; } // 입장에 성공한 방 number
}

[MemoryPackable]
public partial class S_LeaveRoomReq : PkHeader // 방 퇴장 요청 응답
{
    public ErrorCode Result { get; set; }
    public String UserId { get; set; } = string.Empty; // 퇴장한 유저의 user id
}


[MemoryPackable]
public partial class S_RoomChat : PkHeader // 방 채팅 요청 응답
{
    public string UserId { get; set; } = string.Empty;
    public string ChatMsg { get; set; } = string.Empty;
}









// 오목 로직 패킷
[MemoryPackable]
public partial class S_ReadyOmok : PkHeader // 오목 플레이 준비 완료 요청 응답
{
    public string UserId { get; set; } = string.Empty; // 준비 상태가 변경된 유저의 ID
    public bool IsReady { get; set; } // 변경된 준비 완료 여부
}



[MemoryPackable]
public partial class S_StartOmok : PkHeader // 게임 시작 통보 패킷
{
    public string BlackUserId { get; set; } = string.Empty; // 선공인 유저 ID
    public string WhiteUserId { get; set; } = string.Empty; // 선공인 유저 ID
}



[MemoryPackable]
public partial class S_PutMok : PkHeader // 돌 두기 요청 응답
{
    public string UserId { get; set; } = string.Empty;
    public int PosX;
    public int PosY;
}


[MemoryPackable]
public partial class S_EndOmok : PkHeader // 게임 종료 통보 패킷
{
    public string WinUserId { get; set; } = string.Empty; // 승리한 유저 ID
}


