using MemoryPack;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;


namespace GameServer_MemoryPack;

public struct MemoryPackPacketHeader
{
    const int PacketHeaderMemoryPackStartPos = 1;
    public const int HeaderSize = 6;

    public UInt16 TotalSize;
    public UInt16 Id;
    public byte Type;


    public static UInt16 GetTotalSize(byte[] data, int startPos)
    {
        return ReadUInt16(data, startPos + PacketHeaderMemoryPackStartPos);
    }

    public static void WritePacketId(byte[] data, UInt16 packetId)
    {
        WriteUInt16(data, PacketHeaderMemoryPackStartPos + 2, packetId);
    }

    public void Read(byte[] headerData)
    {
        var pos = PacketHeaderMemoryPackStartPos;

        TotalSize = ReadUInt16(headerData, pos);
        pos += 2;

        Id = ReadUInt16(headerData, pos);
        pos += 2;

        Type = headerData[pos];
        pos += 1;
    }
        
    public static void Write(byte[] packetData, PacketId packetId, byte type = 0)
    {
        var pos = PacketHeaderMemoryPackStartPos;

        WriteUInt16(packetData, pos, (UInt16)packetData.Length);
        pos += 2;

        WriteUInt16(packetData, pos, (UInt16)packetId);
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

    static UInt16 ReadUInt16(byte[] data, int pos)
    {
        var bytesSpan1 = data.AsSpan(pos, 2);
        var value = BinaryPrimitives.ReadUInt16LittleEndian(bytesSpan1);
        return value;
    }

    static void WriteUInt16(byte[] data, int pos, UInt16 value)
    {
        var bytesSpan1 = data.AsSpan(pos, 2);
        BinaryPrimitives.WriteUInt16LittleEndian(bytesSpan1, value);
    }   
}



[MemoryPackable]
public partial class PkHeader
{
    public UInt16 TotalSize { get; set; } = 0;
    public UInt16 Id { get; set; } = 0;
    public byte Type { get; set; } = 0;
}


// 에코 요청과 응답
[MemoryPackable]
public partial class PKTReqResEcho : PkHeader
{
    public string DummyData { get; set; }
}


// 로그인 요청
[MemoryPackable]
public partial class PKTReqLogin : PkHeader
{
    public string UserID { get; set; }
    public string AuthToken { get; set; }
}

[MemoryPackable]
public partial class PKTResLogin : PkHeader
{
    public short Result { get; set; }
}

