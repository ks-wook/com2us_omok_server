using GameServer.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public abstract class PacketHandler
{
    public static Func<string, byte[], bool> NetSendFunc;

    public PacketHandler()
    {

    }

    public abstract void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap);
    
}
