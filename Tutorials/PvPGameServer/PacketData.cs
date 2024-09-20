using MemoryPack;
using System;
using System.Collections.Generic;


namespace PvPGameServer;

public struct MemoryPackPacketHeader
{
    const int PacketHeaderMemoryPackStartPos = 1;
    public const int HeaderSize = 6;

    public UInt16 TotalSize;
    public UInt16 Id;
    public byte Type;


    public static UInt16 GetTotalSize(byte[] data, int startPos)
    {
        return FastBinaryRead.UInt16(data, startPos + PacketHeaderMemoryPackStartPos);
    }

    public static void WritePacketId(byte[] data, UInt16 packetId)
    {
        FastBinaryWrite.UInt16(data, PacketHeaderMemoryPackStartPos + 2, packetId);
    }

    public void Read(byte[] headerData)
    {
        var pos = PacketHeaderMemoryPackStartPos;

        TotalSize = FastBinaryRead.UInt16(headerData, pos);
        pos += 2;

        Id = FastBinaryRead.UInt16(headerData, pos);
        pos += 2;

        Type = headerData[pos];
        pos += 1;
    }
        
    public static void Write(byte[] packetData, PacketId packetId, byte type = 0)
    {
        var pos = PacketHeaderMemoryPackStartPos;

        FastBinaryWrite.UInt16(packetData, pos, (UInt16)packetData.Length);
        pos += 2;

        FastBinaryWrite.UInt16(packetData, pos, (UInt16)packetId);
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



[MemoryPackable]
public partial class PkHeader
{
    public UInt16 TotalSize { get; set; } = 0;
    public UInt16 Id { get; set; } = 0;
    public byte Type { get; set; } = 0;
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



[MemoryPackable]
public partial class PKNtfMustClose : PkHeader
{
    public short Result { get; set; }
}



[MemoryPackable]
public partial class PKTReqRoomEnter : PkHeader
{
    public int RoomNumber { get; set; }
}

[MemoryPackable]
public partial class PKTResRoomEnter : PkHeader
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTNtfRoomUserList : PkHeader
{
    public List<string> UserIDList { get; set; } = new List<string>();
}

[MemoryPackable]
public partial class PKTNtfRoomNewUser : PkHeader
{
    public string UserID { get; set; }
}


[MemoryPackable]
public partial class PKTReqRoomLeave : PkHeader
{
}

[MemoryPackable]
public partial class PKTResRoomLeave : PkHeader
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTNtfRoomLeaveUser : PkHeader
{
    public string UserID { get; set; }
}


[MemoryPackable]
public partial class PKTReqRoomChat : PkHeader
{
    public string ChatMessage { get; set; }
}


[MemoryPackable]
public partial class PKTNtfRoomChat : PkHeader
{
    public string UserID { get; set; }

    public string ChatMessage { get; set; }
}
