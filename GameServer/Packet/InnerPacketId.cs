using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Packet;

public enum InnerPacketId
{
    // Heartbeat 2101 ~
    PKTInnerNtfSendPing = 2101,
    PKTInnerNtfCloseConnection = 2102,

    // game 2501 ~
    PKTInnerReqSaveGameResult = 2501,
    PKTInnerResSaveGameResult = 2502,
    PKTInnerNtfTurnChange = 2503,

    // login token 3501 ~ 
    PKTInnerReqVerifyToken = 3501,
    PKTInnerResVerifyToken = 3502,

}
