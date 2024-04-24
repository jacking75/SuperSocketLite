using MemoryPack;
using System;


namespace PvPGameServer;

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
        MemoryPackPacketHeadInfo.Write(sendData, PACKETID.NTF_IN_ROOM_LEAVE);
        
        var memoryPakcPacket = new MemoryPackBinaryRequestInfo(null);
        memoryPakcPacket.Data = sendData;
        memoryPakcPacket.SessionID = sessionID;
        return memoryPakcPacket;
    }

    public static MemoryPackBinaryRequestInfo MakeNTFInConnectOrDisConnectClientPacket(bool isConnect, string sessionID)
    {
        var memoryPakcPacket = new MemoryPackBinaryRequestInfo(null);
        memoryPakcPacket.Data = new byte[MemoryPackPacketHeadInfo.HeadSize];
        
        if (isConnect)
        {
            MemoryPackPacketHeadInfo.WritePacketId(memoryPakcPacket.Data, (UInt16)PACKETID.NTF_IN_CONNECT_CLIENT);
        }
        else
        {
            MemoryPackPacketHeadInfo.WritePacketId(memoryPakcPacket.Data, (UInt16)PACKETID.NTF_IN_DISCONNECT_CLIENT);
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
