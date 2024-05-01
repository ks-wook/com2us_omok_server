using GameServer.DB.Redis;
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

    RedisProcessor _redisProcessor;

    public PacketHandlerAuth(UserManager? userManager, RedisProcessor? redisProcessor)
    {
        if (userManager == null || redisProcessor == null)
        {
            Console.WriteLine("[RoomPacketHandler.Init] roomList null");
            throw new NullReferenceException();
        }

        _userManager = userManager;
        _redisProcessor = redisProcessor;
    }

    public override void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap)
    {
        // 클라이언트 요청
        packetHandlerMap.Add((int)PACKETID.PKTReqLogin, PKTReqLoginHandler);




        // Redis 응답
        packetHandlerMap.Add((int)MQDATAID.MQ_RES_VERIFY_TOKEN, MQResVerifyTokenHandler);
    }



    public void PKTReqLoginHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;


        (ErrorCode result, PKTReqLogin? bodyData) = DeserializePacket<PKTReqLogin>(packet.Data);

        if (result != ErrorCode.None || bodyData == null)
        {
            SendLoginFail(sessionId);
            return;
        }



        result = CheckAuthToken(bodyData, sessionId);

        if(result != ErrorCode.None)
        {
            SendLoginFail(sessionId);
            return;
        }



        // 토큰 인증 성공, 유저 로그인
        // Login(bodyData, sessionId);
    }

    public void MQResVerifyTokenHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;


        (ErrorCode result, MQResVerifyToken? bodyData) = DeserializePacket<MQResVerifyToken>(packet.Data);

        if (result != ErrorCode.None || bodyData == null)
        {
            SendLoginFail(sessionId);
            return;
        }


        Login(bodyData, sessionId);
    }


    public ErrorCode CheckAuthToken(PKTReqLogin bodyData, string sessionId)
    {
        try
        {
            // 레디스에서 토큰 검증
            MQReqVerifyToken sendData = new MQReqVerifyToken();
            sendData.AccountId = Int64.Parse(bodyData.UserId);
            sendData.Token = bodyData.AuthToken;
            SendRedisReqPacket<MQReqVerifyToken>(sendData, MQDATAID.MQ_REQ_VERIFY_TOKEN, sessionId, _redisProcessor);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CheckAuthToken] ErrorCode: {ErrorCode.InvaildToken}, {ex.ToString()}");
            return ErrorCode.InvaildToken;
        }
    }


    public ErrorCode Login(MQResVerifyToken packet, string sessionId)
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
