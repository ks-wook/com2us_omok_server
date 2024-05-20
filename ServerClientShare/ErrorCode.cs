using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public enum ErrorCode
{
    None = 0,

    // Auth 20001 ~
    LoginFail = 20001,
    InvaildToken = 20002,
    AlreadyExsistUser = 20003,
    NullPacket = 20004,
    InvalidRequest = 20005,
    TokenMismatch = 20006,

    // Room 21001 ~ 
    NullRoom = 21001,
    ExceedMaxRoomUser = 21002,
    AlreadyExsistRoomUser = 21003,
    FailLeaveRoom = 21004,
    FailRemoveRoomUser = 21005,
    FailEnterRoom = 21006,

    // User 22001 ~
    NullUser = 22001,
    ExceedMaxUserConnection = 22002,
    RoomChatFail = 22003,

    // OmokGame 23001 ~
    PutMokFail = 23001,
    FailReadyOmok = 23002,


    // ------- Mysql --------
    // GameResult 50001 ~
    FailInsertGameResult = 50001,
    NullUserGameData = 50002,



    // ------- Redis --------
    // Token 60001 ~
    NullLoginToken = 60001,
}
