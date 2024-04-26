using GameServer.Binary;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Packet;




[MemoryPackable]
public partial class C_Test : PkHeader // 테스트용 패킷
{
    public string Msg { get; set; } = string.Empty;
}



[MemoryPackable]
public partial class C_LoginReq : PkHeader // 게임 서버 로그인 요청
{
    public string UID { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
}




[MemoryPackable]
public partial class C_LogoutReq : PkHeader // 게임 서버 로그아웃 요청
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}


[MemoryPackable]
public partial class C_EnterRoomReq : PkHeader // 방 입장 요청
{
    public int RoomNumber = 0; // 0번방은 존재 하지 않으므로 0이라면 잘못된 패킷
}


[MemoryPackable]
public partial class C_LeaveRoomReq : PkHeader // 방 퇴장 요청
{
    public int RoomNumber = 0; // 0번방은 존재 하지 않으므로 0이라면 잘못된 패킷
}


[MemoryPackable]
public partial class C_RoomChat : PkHeader // 방 채팅 요청
{
    public string ChatMsg { get; set; } = string.Empty;
}



// TODO 오목 로직 패킷