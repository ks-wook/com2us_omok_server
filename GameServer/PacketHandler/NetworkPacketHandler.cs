using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.PacketHandler
{
    public class NetworkPacketHandler : PacketHandler
    {

        public override void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap)
        {
            // 테스트 패킷 핸들러 등록
            packetHandlerMap.Add((int)PACKET_ID.C_Test, OnRecvTestPacket);

        }



        // 네트워크관련 패킷 핸들러가 필요하다면 이 아래에 선언한다.
        public void OnRecvTestPacket(MemoryPackBinaryRequestInfo packet)
        {
            var sessionId = packet.SessionID;

            // 테스트 패킷에 대한 로그 작성
            try
            {
                var bodyData = MemoryPackSerializer.Deserialize<PKTTest>(packet.Data);
                
                if (bodyData != null)
                {
                    Console.WriteLine("패킷 수신 성공, MSG: " + bodyData.Msg);
                }
                else
                {
                    Console.WriteLine("패킷 수신 실패");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OnRecvTestPacket] Packet Error " + ex.ToString());
            }
        }


    }
}
