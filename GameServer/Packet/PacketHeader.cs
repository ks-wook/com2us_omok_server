using GameServer.Binary;
using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;





[MemoryPackable]
public partial class PkHeader
{
    public UInt16 TotalSize { get; set; } = 0;
    public UInt16 Id { get; set; } = 0;
    public byte Type { get; set; } = 0;
}




// 수신 패킷 헤더정보 파싱
public struct MemoryPackPacketHeadInfo
{
    const int PacketHeaderMemoryPackStartPos = 1;
    public const int HeadSize = 6;

    public ushort TotalSize;
    public ushort Id;
    public byte Type;

    public static ushort GetTotalSize(byte[] data, int startPos)
    {
        return PtrBinaryRead.UInt16(data, startPos + PacketHeaderMemoryPackStartPos);
    }

    public static void WritePacketId(byte[] data, ushort packetId)
    {
        PtrBinaryWrite.UInt16(data, PacketHeaderMemoryPackStartPos + 2, packetId);
    }

    public void Read(byte[] headerData)
    {
        var pos = PacketHeaderMemoryPackStartPos;

        TotalSize = PtrBinaryRead.UInt16(headerData, pos);
        pos += 2;

        Id = PtrBinaryRead.UInt16(headerData, pos);
        pos += 2;

        Type = headerData[pos];
        pos += 1;
    }

    public static void Write(byte[] packetData, PACKETID packetId, byte type = 0)
    {
        var pos = PacketHeaderMemoryPackStartPos;

        PtrBinaryWrite.UInt16(packetData, pos, (ushort)packetData.Length);
        pos += 2;

        PtrBinaryWrite.UInt16(packetData, pos, (ushort)packetId);
        pos += 2;

        packetData[pos] = type;
    }

    public static void Write(byte[] packetData, InnerPacketId packetId, byte type = 0)
    {
        var pos = PacketHeaderMemoryPackStartPos;

        PtrBinaryWrite.UInt16(packetData, pos, (ushort)packetData.Length);
        pos += 2;

        PtrBinaryWrite.UInt16(packetData, pos, (ushort)packetId);
        pos += 2;

        packetData[pos] = type;
    }



    public void DebugConsolOutHeaderInfo()
    {
        Console.WriteLine("DebugConsolOutHeaderInfo");
        Console.WriteLine("TotalSize : " + TotalSize);
        Console.WriteLine("Id : " + Id);
        Console.WriteLine("Type : " + Type);
    }
}


