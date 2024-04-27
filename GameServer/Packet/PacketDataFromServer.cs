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

    public Int32 RoomNumber { get; set; } // 입장에 성공한 방 number
}

[MemoryPackable]
public partial class S_LeaveRoomReq : PkHeader // 방 퇴장 요청 응답
{
    public ErrorCode Result { get; set; }
}


[MemoryPackable]
public partial class S_RoomChat : PkHeader // 방 채팅 요청 응답
{
    public string UserId { get; set; } = string.Empty;
    public string ChatMsg { get; set; } = string.Empty;
}

// TODO 오목 로직 패킷


