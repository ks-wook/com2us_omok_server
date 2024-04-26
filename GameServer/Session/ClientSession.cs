using GameServer.Packet;
using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Session
{
    public class ClientSession : AppSession<ClientSession, MemoryPackBinaryRequestInfo>
    {
    }
}
