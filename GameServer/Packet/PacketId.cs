namespace GameServer;

public enum PACKETID : short
{
    // Auth 1001 ~
    PKTReqLogin = 1001,
    PKTResLogin = 1002,

    // Hertbeat 1501 ~
    PKTReqPing = 1501,
    PKTResPing = 1502,

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
