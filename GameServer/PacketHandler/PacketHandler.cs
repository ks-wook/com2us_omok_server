using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public abstract class PacketHandler
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

    // 실패 패킷 전송
    public void SendFailPacket<T>(PACKETID packetId, string sessionId, ErrorCode error) where T : PacketResult, new()
    {
        T sendData = new T();
        sendData.Result = error;

        var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);
        NetSendFunc(sessionId, sendPacket);
    }


    public abstract void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap);
    
}
