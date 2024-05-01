using GameServer.DB.Mysql;
using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class PacketHandler
{
    public static Func<string, byte[], bool> NetSendFunc;

    public PacketHandler()
    {

    }

    public (ErrorCode, T?) DeserializePacket<T>(byte[] bytes)
    {
        T? bodyData = MemoryPackSerializer.Deserialize<T>(bytes);
        if (bodyData == null)
        {
            Console.WriteLine($"[DesrializePacket] ErrorCode: {ErrorCode.NullPacket}");
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

    // DB완료 패킷 전송
    public void SendDBPacket<T>(T sendData, MQDATAID packetId, string sessionId, PacketProcessor packetProcessor)
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

    // DB 실패 패킷 전송
    public void SendDBFailPacket<T>(MQDATAID packetId, PacketProcessor packetProcessor, ErrorCode error) where T : PacketResult, new()
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
    public virtual void RegisterPacketHandler(Dictionary<int, Func<byte[], Task>> packetHandlerMap) { }
    
}
