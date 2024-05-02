using GameServer;
using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace GameServer;


public class BasePacketHandler
{
    public static Func<string, byte[], bool> NetSendFunc;

    public BasePacketHandler()
    {

    }

    public (ErrorCode, T?) DeserializePacket<T>(byte[] bytes)
    {
        T? bodyData = MemoryPackSerializer.Deserialize<T>(bytes);
        if (bodyData == null)
        {
            MainServer.MainLogger.Error($"[DesrializePacket] ErrorCode: {ErrorCode.NullPacket}");
            return (ErrorCode.NullPacket, bodyData);
        }

        return (ErrorCode.None, bodyData);
    }




    // 일반 패킷 전송
    public void SendPacket<T>(T sendData, PACKETID packetId, string sessionId)
    {
        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        NetSendFunc(sessionId, sendPacket);
    }





    // Mysql 완료 패킷 전송
    public void SendMysqlResPacket<T>(T sendData, InnerPacketId packetId, string sessionId, PacketProcessor packetProcessor)
    {
        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        MemoryPackBinaryRequestInfo memoryPackReq = new MemoryPackBinaryRequestInfo(sendPacket);
        memoryPackReq.SessionID = sessionId;
        packetProcessor.Insert(memoryPackReq);
    }

    // Mysql 요청 패킷 전송
    public void SendMysqlReqPacket<T>(T sendData, InnerPacketId packetId, string sessionId, MysqlProcessor packetProcessor)
    {
        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        MemoryPackBinaryRequestInfo memoryPackReq = new MemoryPackBinaryRequestInfo(sendPacket);
        memoryPackReq.SessionID = sessionId;
        packetProcessor.Insert(memoryPackReq);
    }







    // Redis 요청 패킷 전송
    public void SendRedisReqPacket<T>(T sendData, InnerPacketId packetId, string sessionId, RedisProcessor packetProcessor)
    {
        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        MemoryPackBinaryRequestInfo memoryPackReq = new MemoryPackBinaryRequestInfo(sendPacket);
        memoryPackReq.SessionID = sessionId;
        packetProcessor.Insert(memoryPackReq);
    }


    // Redis 완료 패킷 전송
    public void SendRedisResPacket<T>(T sendData, InnerPacketId packetId, string sessionId, PacketProcessor packetProcessor)
    {
        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        MemoryPackBinaryRequestInfo memoryPackReq = new MemoryPackBinaryRequestInfo(sendPacket);
        memoryPackReq.SessionID = sessionId;
        packetProcessor.Insert(memoryPackReq);
    }









    // 실패 패킷 전송
    public void SendFailPacket<T>(PACKETID packetId, string sessionId, ErrorCode error) where T : PacketResult, new()
    {
        T sendData = new T();
        sendData.Result = error;

        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        NetSendFunc(sessionId, sendPacket);
    }

    // Mysql 실패 패킷 전송
    public void SendMysqlFailPacket<T>(InnerPacketId packetId, PacketProcessor packetProcessor, ErrorCode error) where T : PacketResult, new()
    {
        T sendData = new T();
        sendData.Result = error;

        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        packetProcessor.Insert(new MemoryPackBinaryRequestInfo(sendPacket));
    }

    // Redis 실패 패킷 전송
    public void SendRedisFailPacket<T>(InnerPacketId packetId, PacketProcessor packetProcessor, ErrorCode error) where T : PacketResult, new()
    {
        T sendData = new T();
        sendData.Result = error;

        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        packetProcessor.Insert(new MemoryPackBinaryRequestInfo(sendPacket));
    }




    public void SendFailPacket<T>(PACKETID packetId, List<string> sessionIds, ErrorCode error) where T : PacketResult, new()
    {
        T sendData = new T();
        sendData.Result = error;

        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);


        foreach (var sessionId in sessionIds)
        {
            NetSendFunc(sessionId, sendPacket);
        }
    }



    public virtual void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap) { }
    public virtual void RegisterPacketHandler(Dictionary<int, Func<MemoryPackBinaryRequestInfo, Task>> packetHandlerMap) { }
    
}
