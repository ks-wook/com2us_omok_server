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

    public (ErrorCode, T?) DeserializeNullablePacket<T>(byte[] bytes)
    {
        T? bodyData = MemoryPackSerializer.Deserialize<T>(bytes);
        if (bodyData == null)
        {
            MainServer.MainLogger.Error($"[DesrializePacket] ErrorCode: {ErrorCode.NullPacket}");
            return (ErrorCode.NullPacket, bodyData);
        }

        return (ErrorCode.None, bodyData);
    }


#pragma warning disable 8603  
    public T DeserializePacket<T>(byte[] bytes)
    {
        return MemoryPackSerializer.Deserialize<T>(bytes);
    }
#pragma warning restore 8603 


    // 일반 패킷 전송
    public void SendPacket<T>(T sendData, PACKETID packetId, string sessionId)
    {
        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        NetSendFunc(sessionId, sendPacket);
    }



    // Mysql 요청 패킷 전송
    public void SendInnerReqPacket<T>(T sendData, InnerPacketId packetId, string sessionId, MysqlProcessor packetProcessor)
    {
        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        MemoryPackBinaryRequestInfo memoryPackReq = new MemoryPackBinaryRequestInfo(sendPacket);
        memoryPackReq.SessionID = sessionId;
        packetProcessor.Insert(memoryPackReq);
    }

    // Redis 요청 패킷 전송
    public void SendInnerReqPacket<T>(T sendData, InnerPacketId packetId, string sessionId, RedisProcessor packetProcessor)
    {
        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        MemoryPackBinaryRequestInfo memoryPackReq = new MemoryPackBinaryRequestInfo(sendPacket);
        memoryPackReq.SessionID = sessionId;
        packetProcessor.Insert(memoryPackReq);
    }







    // Inner 패킷 요청 완료 전송
    public void SendInnerResPacket<T>(T sendData, InnerPacketId packetId, string sessionId, PacketProcessor packetProcessor)
    {
        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        MemoryPackBinaryRequestInfo memoryPackReq = new MemoryPackBinaryRequestInfo(sendPacket);
        memoryPackReq.SessionID = sessionId;
        packetProcessor.Insert(memoryPackReq);
    }







    // 실패 패킷 전송
    public void SendFailPacket<T>(PACKETID packetId, string sessionId, ErrorCode error) where T : PkResult, new()
    {
        T sendData = new T();
        sendData.Result = error;

        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        NetSendFunc(sessionId, sendPacket);
    }

    // Mysql 실패 패킷 전송
    public void SendMysqlFailPacket<T>(InnerPacketId packetId, PacketProcessor packetProcessor, ErrorCode error) where T : PkResult, new()
    {
        T sendData = new T();
        sendData.Result = error;

        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        packetProcessor.Insert(new MemoryPackBinaryRequestInfo(sendPacket));
    }

    // Redis 실패 패킷 전송
    public void SendRedisFailPacket<T>(InnerPacketId packetId, PacketProcessor packetProcessor, ErrorCode error) where T : PkResult, new()
    {
        T sendData = new T();
        sendData.Result = error;

        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        packetProcessor.Insert(new MemoryPackBinaryRequestInfo(sendPacket));
    }




    public void SendFailPacket<T>(PACKETID packetId, List<string> sessionIds, ErrorCode error) where T : PkResult, new()
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
