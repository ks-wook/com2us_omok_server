using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class NetworkPacketHandler : PacketHandler
{
    UserManager? _userManager;

    public void Init(UserManager userManger)
    {
        if (userManger == null)
        {
            Console.WriteLine("[NetworkPacketHandler.Init] UserManager null");
            return;
        }

        _userManager = userManger;
    }

    public override void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap)
    {
        // 테스트 패킷 핸들러 등록
        packetHandlerMap.Add((int)PACKET_ID.C_Test, OnRecvTestPacket);



        packetHandlerMap.Add((int)PACKET_ID.C_LoginReq, C_LoginReqHander);

    }


    // TEST 테스트 패킷 핸들러
    public void OnRecvTestPacket(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        // 테스트 패킷에 대한 로그 작성
        try
        {
            var bodyData = MemoryPackSerializer.Deserialize<C_Test>(packet.Data);

            if (bodyData != null)
            {
                Console.WriteLine("패킷 수신 성공, MSG: " + bodyData.Msg);


                // 테스트 패킷 재전송
                var testPacket = new S_Test();
                testPacket.Msg = "Hello Client";

                var sendBodyData = MemoryPackSerializer.Serialize(testPacket);
                MemoryPackPacketHeadInfo.Write(sendBodyData, PACKET_ID.S_Test);
                NetSendFunc(sessionId, sendBodyData);


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





    public void C_LoginReqHander(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;


        try
        {
            // TODO 게임 레디스에서 토큰 검증









            dynamic? bodyData = MemoryPackSerializer.Deserialize<C_LoginReq>(packet.Data);
            if(bodyData == null)
            {
                Console.WriteLine($"[C_LoginReqHander] ErrorCode: {ErrorCode.NullPacket}");
                return;
            }

            // 유저 매니저에 추가하고 서버 접속 처리
            ErrorCode result = _userManager.AddUser(bodyData.UserId, sessionId);


            if(result != ErrorCode.None)
            {
                Console.WriteLine($"[C_LoginReqHander] ErrorCode: {result}");
                return;
            }




            // 로그인 성공 응답
            {
                S_LoginReq sendData = new S_LoginReq();
                var sendPacket = MemoryPackSerializer.Serialize<S_LoginReq>(sendData);
                MemoryPackPacketHeadInfo.Write(sendPacket, PACKET_ID.S_LoginReq);
                NetSendFunc(sessionId, sendPacket);
            }
            


        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ErrorCode]: {ErrorCode.LoginFail}, {ex.ToString()}");
        }


    }

}
