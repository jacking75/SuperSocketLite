using MemoryPack;
using System;


namespace PvPGameServer;

// 서버 내부에서 사용하는 패킷을 만드는 클래스
public class InnerPakcetMaker
{
    public static MemoryPackBinaryRequestInfo MakeNTFInnerRoomLeavePacket(string sessionID, int roomNumber, string userID)
    {            
        var packet = new PKTInternalNtfRoomLeave()
        {
            RoomNumber = roomNumber,
            UserID = userID,
        };

        var sendData = MemoryPackSerializer.Serialize(packet);
        MemoryPackPacketHeader.Write(sendData, PacketId.NtfInRoomLeave);
        
        var memoryPakcPacket = new MemoryPackBinaryRequestInfo(null);
        memoryPakcPacket.Data = sendData;
        memoryPakcPacket.SessionID = sessionID;
        return memoryPakcPacket;
    }

    public static MemoryPackBinaryRequestInfo MakeNTFInConnectOrDisConnectClientPacket(bool isConnect, string sessionID)
    {
        var memoryPakcPacket = new MemoryPackBinaryRequestInfo(null);
        memoryPakcPacket.Data = new byte[MemoryPackPacketHeader.HeaderSize];
        
        if (isConnect)
        {
            MemoryPackPacketHeader.WritePacketId(memoryPakcPacket.Data, (UInt16)PacketId.NtfInConnectClient);
        }
        else
        {
            MemoryPackPacketHeader.WritePacketId(memoryPakcPacket.Data, (UInt16)PacketId.NtfInDisconnectClient);
        }

        memoryPakcPacket.SessionID = sessionID;
        return memoryPakcPacket;
    }

}
   

[MemoryPackable]
public partial class PKTInternalNtfRoomLeave : PkHeader
{
    public int RoomNumber { get; set; }
    public string UserID { get; set; }
}
