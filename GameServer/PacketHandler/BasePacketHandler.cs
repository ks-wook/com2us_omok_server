using GameServer;
using GameServer.Packet;
using MemoryPack;
using MySqlConnector;
using SqlKata.Execution;
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

    public (ErrorCode, T?) DeserializeNullablePacket<T>(byte[] bytes)
    {
        T? bodyData = MemoryPackSerializer.Deserialize<T>(bytes);
        if (bodyData == null)
        {
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

    // 실패 패킷 전송
    public void SendFailPacket<T>(PACKETID packetId, string sessionId, ErrorCode error) where T : PkResult, new()
    {
        T sendData = new T();
        sendData.Result = error;

        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        NetSendFunc(sessionId, sendPacket);
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

    // Inner 요청 패킷 전송
    public void SendInnerReqPacket<T>(T sendData, InnerPacketId packetId, string sessionId, Action<MemoryPackBinaryRequestInfo> InsertAction)
    {
        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        MemoryPackBinaryRequestInfo memoryPackReq = new MemoryPackBinaryRequestInfo(sendPacket);
        memoryPackReq.SessionID = sessionId;
        InsertAction(memoryPackReq);
    }

    // Inner 패킷 요청 완료 전송
    public void SendInnerResPacket<T>(T sendData, InnerPacketId packetId, string sessionId, Action<MemoryPackBinaryRequestInfo> mainPacketInsert)
    {
        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        MemoryPackBinaryRequestInfo memoryPackReq = new MemoryPackBinaryRequestInfo(sendPacket);
        memoryPackReq.SessionID = sessionId;
        mainPacketInsert(memoryPackReq);
    }

    // Inner 실패 패킷 전송
    public void SendInnerFailPacket<T>(InnerPacketId packetId, ErrorCode error, Action<MemoryPackBinaryRequestInfo> mainPacketInsert) where T : PkResult, new()
    {
        T sendData = new T();
        sendData.Result = error;

        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        mainPacketInsert(new MemoryPackBinaryRequestInfo(sendPacket));
    }

    public virtual void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap) { }
    public virtual void RegisterPacketHandler(Dictionary<int, Func<MemoryPackBinaryRequestInfo, Task>> packetHandlerMap) { }
    public virtual void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo, QueryFactory>> packetHandlerMap) { }
}