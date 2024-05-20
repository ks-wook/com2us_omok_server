using GameServer.Packet;
using MemoryPack;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class PacketHandlerUser : BasePacketHandler
{
    ILog _logger;

    UserManager _userManager;

    Action<MemoryPackBinaryRequestInfo> _redisInsert;

    public PacketHandlerUser(ILog logger, UserManager? userManager, Action<MemoryPackBinaryRequestInfo> redisInsert)
    {
        _logger = logger;

        if (userManager == null || redisInsert == null)
        {
            _logger.Error("[PacketHandlerUser] roomList null");
            throw new NullReferenceException();
        }

        _userManager = userManager;
        _redisInsert = redisInsert;
    }

    public override void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap)
    {
        // 클라이언트
        packetHandlerMap.Add((int)PACKETID.PKTReqLogin, PKTReqLoginHandler);
        packetHandlerMap.Add((int)PACKETID.PKTResPing, PKTResPingHandler);


        // Inner Packet
        packetHandlerMap.Add((int)InnerPacketId.PKTInnerResVerifyToken, MQResVerifyTokenHandler);
        packetHandlerMap.Add((int)InnerPacketId.PKTInnerNtfSendPing, PKTInnerNtfSendPingHandler);
        packetHandlerMap.Add((int)InnerPacketId.PKTInnerNtfCloseConnection, PKTInnerNtfCloseConnectionHandler);
    }

    public void PKTReqLoginHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;


        (ErrorCode result, PKTReqLogin? bodyData) = DeserializeNullablePacket<PKTReqLogin>(packet.Data);

        if (result != ErrorCode.None || bodyData == null)
        {
            SendLoginFail(sessionId);
            return;
        }

        result = CheckAuthToken(bodyData, sessionId);

        if (result != ErrorCode.None)
        {
            SendLoginFail(sessionId);
            return;
        }
    }

    public void MQResVerifyTokenHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;


        (ErrorCode result, PKTInnerResVerifyToken? bodyData) = DeserializeNullablePacket<PKTInnerResVerifyToken>(packet.Data);

        if (result != ErrorCode.None || bodyData == null)
        {
            SendLoginFail(sessionId);
            return;
        }

        if(bodyData.Result != ErrorCode.None)
        {
            SendLoginFail(sessionId);
            return;
        }

        Login(bodyData, sessionId);
    }

    public void PKTResPingHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        CheckUserState(sessionId);
    }

    public void PKTInnerNtfSendPingHandler(MemoryPackBinaryRequestInfo packet)
    {
        var bodyData = DeserializePacket<PKTInnerNtfSendPing>(packet.Data);

        SendPing(bodyData.SessionId);
    }

    public void PKTInnerNtfCloseConnectionHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;
        
        var bodyData = DeserializePacket<PKTInnerNtfCloseConnection>(packet.Data);
        
        _userManager.CloseConnectionBySessionId(bodyData.SessionId);
    }

    public ErrorCode CheckAuthToken(PKTReqLogin bodyData, string sessionId)
    {
        try
        {
            // 레디스에서 토큰 검증
            PKTInnerReqVerifyToken sendData = new PKTInnerReqVerifyToken();
            sendData.AccountId = Int64.Parse(bodyData.UserId);
            sendData.Token = bodyData.AuthToken;
            SendInnerReqPacket<PKTInnerReqVerifyToken>(sendData, InnerPacketId.PKTInnerReqVerifyToken, sessionId, _redisInsert);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.Error($"[CheckAuthToken] ErrorCode: {ErrorCode.InvaildToken}, {ex.ToString()}");
            return ErrorCode.InvaildToken;
        }
    }

    public ErrorCode Login(PKTInnerResVerifyToken packet, string sessionId)
    {
        // 유저 매니저에 추가하고 서버 접속 처리
        ErrorCode result = _userManager.AddUser(packet.UserId, sessionId);

        if (result != ErrorCode.None)
        {
            _logger.Error($"[Login] ErrorCode: {result}");
            return result;
        }

        PKTResLogin sendData = new PKTResLogin();
        sendData.UserId = packet.UserId;
        var sendPacket = MemoryPackSerializer.Serialize<PKTResLogin>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, PACKETID.PKTResLogin);
        NetSendFunc(sessionId, sendPacket);

        return ErrorCode.None;
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
    
    public void CheckUserState(string sessionId)
    {
        User? user = _userManager.GetUserBySessionId(sessionId);
        if (user == null)
        {
            _logger.Error($"찾을 수 없는 유저의 Ping 응답");
            return;
        }

        // 응답 시간 업데이트
        user.UpdateLastPingCheckTime();
    }

    public void SendPing(string sessionId)
    {
        PKTReqPing sendData = new PKTReqPing();
        SendPacket<PKTReqPing>(sendData, PACKETID.PKTReqPing, sessionId);
    }
}
