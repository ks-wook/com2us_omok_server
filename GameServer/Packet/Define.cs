namespace GameServer.Packet
{
    public enum PACKET_ID
    {
        C_Test = 101, // 테스트 요청 패킷

        // 네트워크 1001 ~
        C_LoginReq = 1001,
        C_LogoutReq = 1002,


        // Room 2001 ~
        C_EnterRoomReq = 2001,
        C_LeaveRoomReq = 2002,
        C_RoomChat = 2003,


    }
}
