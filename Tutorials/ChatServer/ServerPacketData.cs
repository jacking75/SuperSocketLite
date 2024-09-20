using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSBaseLib;
using MessagePack;

namespace ChatServer;

// 서버 내부에서 사용되는 패킷 객체
public class ServerPacketData
{
    public Int16 PacketSize;
    public string SessionID; 
    public Int16 PacketID;        
    public SByte Type;

    public byte[] BodyData;
            
    
    public void SetPacketData(string sessionID, Int16 packetID, byte[] packetBodyData)
    {
        SessionID = sessionID;
        PacketID = packetID;
        
        if (packetBodyData.Length > 0)
        {
            BodyData = packetBodyData;
        }
    }
            
    // 클라이언트 연결/끊어짐을 알리는 내부 패킷 생성
    public static ServerPacketData MakeNTFInConnectOrDisConnectClientPacket(bool isConnect, string sessionID)
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
