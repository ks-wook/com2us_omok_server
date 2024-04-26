using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.PacketHandler
{
    public abstract class PacketHandler
    {
        public PacketHandler()
        {

        }

        public abstract void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap);

    }
}
