using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSBaseLib;
using MessagePack;


namespace GameServer;

public class ServerPacketData
{
    public UInt16 PacketSize;
    public string SessionID; 
    public int SessionIndex;
    public UInt16 PacketID;        
    public SByte Type;
    public byte[] BodyData;
            
    
    public void Assign(string sessionID, int sessionIndex, UInt16 packetID, byte[] packetBodyData)
    {
        SessionIndex = sessionIndex;
        SessionID = sessionID;

        PacketID = packetID;
        
        if (packetBodyData.Length > 0)
        {
            BodyData = packetBodyData;
        }
    }
            
    public static ServerPacketData MakeNTFInConnectOrDisConnectClientPacket(bool isConnect, string sessionID, int sessionIndex)
    {
        var packet = new ServerPacketData();
        
        if (isConnect)
        {
            packet.PacketID = (Int32)PacketId.NtfInConnectClient;
        }
        else
        {
            packet.PacketID = (Int32)PacketId.NtfInDisconnectClient;
        }

        packet.SessionIndex = sessionIndex;
        packet.SessionID = sessionID;
        return packet;
    }               
    
}



[MessagePackObject]
public class PKTInternalReqRoomEnter
{
    [Key(0)]
    public int RoomNumber;

    [Key(1)]
    public string UserID;        
}

[MessagePackObject]
public class PKTInternalResRoomEnter
{
    [Key(0)]
    public ErrorCode Result;

    [Key(1)]
    public int RoomNumber;

    [Key(2)]
    public string UserID;
}


[MessagePackObject]
public class PKTInternalNtfRoomLeave
{
    [Key(0)]
    public int RoomNumber;

    [Key(1)]
    public string UserID;
}
