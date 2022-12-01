using MemoryPack;

namespace TestMemoryPack;


/*public class PacketDef
{
    public const Int16 MEMORYPACK_PACKET_HEADER_SIZE = 8;

    public const Int16 PACKET_HEADER_SIZE = 8;
    public const int MAX_USER_ID_BYTE_LENGTH = 16;
    public const int MAX_USER_PW_BYTE_LENGTH = 16;

    public const int INVALID_ROOM_NUMBER = -1;
}*/

public struct MemoryPackPacketHeadInfo
{
    const int PacketHeaderMemoryPackStartPos = 1;
    public const int HeadSize = 6;

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

    public void Write(byte[] mqData)
    {
        var pos = PacketHeaderMemoryPackStartPos;

        FastBinaryWrite.UInt16(mqData, pos, TotalSize);
        pos += 2;

        FastBinaryWrite.UInt16(mqData, pos, Id);
        pos += 2;

        mqData[pos] = Type;
        pos += 1;
    }

    
    public void DebugConsolOutHeaderInfo()
    {
        Console.WriteLine("TotalSize : " + TotalSize);
        Console.WriteLine("Id : " + Id);
        Console.WriteLine("Type : " + Type);
    }   
}


public class PacketToBytes
{
    public static byte[] Make(Int16 packetID, byte[] bodyData)
    {
        byte type = 0;
        var pktID = packetID;
        Int16 bodyDataSize = 0;
        if (bodyData != null)
        {
            bodyDataSize = (Int16)bodyData.Length;
        }
        var packetSize = (Int16)(bodyDataSize + MemoryPackPacketHeadInfo.HeadSize);

        var dataSource = new byte[packetSize];
        Buffer.BlockCopy(BitConverter.GetBytes(packetSize), 0, dataSource, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(pktID), 0, dataSource, 2, 2);
        dataSource[4] = type;

        if (bodyData != null)
        {
            Buffer.BlockCopy(bodyData, 0, dataSource, 5, bodyDataSize);
        }

        return dataSource;
    }

    public static Tuple<int, byte[]> ClientReceiveData(int recvLength, byte[] recvData)
    {
        var packetSize = BitConverter.ToInt16(recvData, 0);
        var packetID = BitConverter.ToInt16(recvData, 2);
        var bodySize = packetSize - MemoryPackPacketHeadInfo.HeadSize;

        var packetBody = new byte[bodySize];
        Buffer.BlockCopy(recvData, MemoryPackPacketHeadInfo.HeadSize, packetBody, 0, bodySize);

        return new Tuple<int, byte[]>(packetID, packetBody);
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
    public string UserID { get; set; } = default!;
    public string AuthToken { get; set; } = default!;
}

[MemoryPackable]
public partial class PKTResRoomEnter : PkHeader
{
    public Int16 ErrorCode { get; set; }
    public int RoomNumber { get; set; }
}