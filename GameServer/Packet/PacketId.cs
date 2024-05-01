namespace GameServer;

// 패킷 명명 규칙
// 1. 클라이언트 -> 서버 방향으로 보내는 패킷은 C_
// 2. 서버 -> 클라이언트 방향으로 보내느 패킷은 S_
// -> 요약하면 요청은 C_, 응답은 S_ 로 패킷이름을 붙인다.


public enum PACKETID : short
{
    // Auth 1001 ~
    PKTReqLogin = 1001,
    PKTResLogin = 1002,


    // Room 2001 ~
    PKTReqRoomEnter = 2001,
    PKTResRoomEnter = 2002,
    PKTReqRoomLeave = 2003,
    PKTResRoomLeave = 2004,
    PKTReqRoomChat = 2005,
    PKTResRoomChat = 2006,


    // Omok Logic 3001 ~
    PKTReqReadyOmok = 3001,
    PKTResReadyOmok = 3002,
    PKTNtfStartOmok = 3003,
    PKTReqPutMok = 3004,
    PKTResPutMok = 3005,
    PKTNtfEndOmok = 3006,




}
