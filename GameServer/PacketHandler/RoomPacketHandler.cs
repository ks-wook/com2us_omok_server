using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.PacketHandler
{
    public class RoomPacketHandler : PacketHandler
    {
        public override void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap)
        {


            throw new NotImplementedException();
        }



        // TODO 방 입장, 방 퇴장, 방 채팅







    }
}
