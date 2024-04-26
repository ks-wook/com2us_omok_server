using GameServer.Binary;
using GameServer.Packet;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class MemoryPackBinaryRequestInfo : BinaryRequestInfo
{
    public string SessionID;
    public byte[] Data;

    public const int PACKET_HEADER_MEMORYPACK_START_POS = 1;
    public const int HEADERE_SIZE = 5 + PACKET_HEADER_MEMORYPACK_START_POS;

    public MemoryPackBinaryRequestInfo(byte[] packetData)
        : base(null, packetData)
    {
        Data = packetData;
    }

}

public struct MemoryPackPacketHeadInfo
{
    const int PacketHeaderMemoryPackStartPos = 1;
    public const int HeadSize = 6;

    public UInt16 TotalSize;
    public UInt16 Id;
    public byte Type;

    public static UInt16 GetTotalSize(byte[] data, int startPos)
    {
        return PtrBinaryReader.UInt16(data, startPos + PacketHeaderMemoryPackStartPos);
    }

    public static void WritePacketId(byte[] data, UInt16 packetId)
    {
        PtrBinaryWriter.UInt16(data, PacketHeaderMemoryPackStartPos + 2, packetId);
    }

    public void Read(byte[] headerData)
    {
        var pos = PacketHeaderMemoryPackStartPos;

        TotalSize = PtrBinaryReader.UInt16(headerData, pos);
        pos += 2;

        Id = PtrBinaryReader.UInt16(headerData, pos);
        pos += 2;

        Type = headerData[pos];
        pos += 1;
    }

    public static void Write(byte[] packetData, PACKET_ID packetId, byte type = 0)
    {
        var pos = PacketHeaderMemoryPackStartPos;

        PtrBinaryWriter.UInt16(packetData, pos, (UInt16)packetData.Length);
        pos += 2;

        PtrBinaryWriter.UInt16(packetData, pos, (UInt16)packetId);
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


public class ReceiveFilter : FixedHeaderReceiveFilter<MemoryPackBinaryRequestInfo>
{
    public ReceiveFilter() : base(MemoryPackBinaryRequestInfo.HEADERE_SIZE)
    {
    }

    protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(header, offset, 2);
        }

        var totalSize = BitConverter.ToUInt16(header, offset + MemoryPackBinaryRequestInfo.PACKET_HEADER_MEMORYPACK_START_POS);
        return totalSize - MemoryPackBinaryRequestInfo.HEADERE_SIZE;
    }

    // offset: header 데이터까지 있는 readBuffer 에서 body가 시작되는 위치를 가리킨다
    // length: body 데이터의 크기 
    protected override MemoryPackBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] readBuffer, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(header.Array, 0, MemoryPackBinaryRequestInfo.HEADERE_SIZE);
        }

        // body 데이터가 있는 경우
        if (length > 0)
        {
            if (offset >= MemoryPackBinaryRequestInfo.HEADERE_SIZE)
            {
                var packetStartPos = offset - MemoryPackBinaryRequestInfo.HEADERE_SIZE;
                var packetSize = length + MemoryPackBinaryRequestInfo.HEADERE_SIZE;

                return new MemoryPackBinaryRequestInfo(readBuffer.CloneRange(packetStartPos, packetSize));
            }
            else
            {
                //offset 이 헤더 크기보다 작으므로 헤더와 보디를 직접 합쳐야 한다.
                var packetData = new Byte[length + MemoryPackBinaryRequestInfo.HEADERE_SIZE];
                header.CopyTo(packetData, 0);
                Array.Copy(readBuffer, offset, packetData, MemoryPackBinaryRequestInfo.HEADERE_SIZE, length);

                return new MemoryPackBinaryRequestInfo(packetData);
            }
        }

        // body 데이터가 없는 경우
        return new MemoryPackBinaryRequestInfo(header.CloneRange(header.Offset, header.Count));
    }


}
