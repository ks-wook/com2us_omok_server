using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class PacketHandlerAuth : PacketHandler
{
    UserManager _userManager;

    public PacketHandlerAuth(UserManager? userManager)
    {
        if (userManager == null)
        {
            Console.WriteLine("[RoomPacketHandler.Init] roomList null");
            throw new NullReferenceException();
        }

        this._userManager = userManager;
    }

    public override void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap)
    {
        // 테스트 패킷 핸들러 등록
        packetHandlerMap.Add((int)PACKETID.C_Test, OnRecvTestPacket);



        packetHandlerMap.Add((int)PACKETID.PKTReqLogin, C_LoginReqHander);

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
                MemoryPackPacketHeadInfo.Write(sendBodyData, PACKETID.S_Test);
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

        PKTReqLogin? bodyData = MemoryPackSerializer.Deserialize<PKTReqLogin>(packet.Data);
        if (bodyData == null)
        {
            Console.WriteLine($"[C_LoginReqHander] ErrorCode: {ErrorCode.NullPacket}");
            SendLoginFail(sessionId);
            return;
        }

        ErrorCode result = CheckAuthToken(bodyData, sessionId);
        if(result != ErrorCode.None)
        {
            return;
        }


        // 토큰 인증 성공, 유저 로그인
        Login(bodyData, sessionId);
    }


    public ErrorCode CheckAuthToken(PKTReqLogin? bodyData, string sessionId)
    {
        
        try
        {
            // TODO 레디스에서 토큰 검증















            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CheckAuthToken] ErrorCode: {ErrorCode.InvaildToken}, {ex.ToString()}");
            SendLoginFail(sessionId);
            return ErrorCode.InvaildToken;
        }
    }


    public ErrorCode Login(PKTReqLogin packet, string sessionId)
    {
        try
        {
            // 유저 매니저에 추가하고 서버 접속 처리
            ErrorCode result = _userManager.AddUser(packet.UserId, sessionId);

            if (result != ErrorCode.None)
            {
                Console.WriteLine($"[C_LoginReqHander] ErrorCode: {result}");
                return result;
            }

            PKTResLogin sendData = new PKTResLogin();
            sendData.UserId = packet.UserId;
            var sendPacket = MemoryPackSerializer.Serialize<PKTResLogin>(sendData);
            MemoryPackPacketHeadInfo.Write(sendPacket, PACKETID.PKTResLogin);
            NetSendFunc(sessionId, sendPacket);

            return ErrorCode.None;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[Login] ErrorCode : {ErrorCode.LoginFail}, {ex.ToString()}");
            SendLoginFail(sessionId);
            return ErrorCode.LoginFail;
        }
        
    }

    // 로그인 실패 패킷 전송
    public void SendLoginFail(string sessionId)
    {
        // 로그인 실패 패킷 전송
        PKTResLogin sendData = new PKTResLogin();
        sendData.Result = ErrorCode.LoginFail;
        var sendPacket = MemoryPackSerializer.Serialize<PKTResLogin>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, PACKETID.PKTResLogin);
        NetSendFunc(sessionId, sendPacket);
    }
}
