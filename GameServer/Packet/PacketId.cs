namespace GameServer.Packet
{
    // 패킷 명명 규칙
    // 1. 클라이언트 -> 서버 방향으로 보내는 패킷은 C_
    // 2. 서버 -> 클라이언트 방향으로 보내느 패킷은 S_
    // -> 요약하면 요청은 C_, 응답은 S_ 로 패킷이름을 붙인다.


    public enum PACKET_ID : short
    {
        C_Test = 101, // 테스트 요청 패킷
        S_Test = 102, // 테스트 요청 읃답

        // 네트워크 1001 ~
        C_LoginReq = 1001,
        S_LoginReq = 1002,
        C_LogoutReq = 1003,
        S_LogoutReq = 1004,


        // Room 2001 ~
        C_EnterRoomReq = 2001,
        S_EnterRoomReq = 2002,
        C_LeaveRoomReq = 2003,
        S_LeaveRoomReq = 2004,
        C_RoomChat = 2005,
        S_RoomChat = 2006,


        // Omok Logic 3001 ~
        C_ReadyOmok = 3001,
        S_ReadyOmok = 3002,

        S_StartOmok = 3003,
        
        C_PutMok = 3004,
        S_PutMok = 3005,

        S_EndOmok = 3006,




    }
}
