using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;

namespace PvPGameServer
{
    public class ServerPacketData
    {
        public Int16 PacketSize;
        public string SessionID; 
        public Int16 PacketID;        
        public SByte Type;
        public byte[] BodyData;
                
        
        public void Assign(string sessionID, Int16 packetID, byte[] packetBodyData)
        {
            SessionID = sessionID;
            PacketID = packetID;
            
            if (packetBodyData.Length > 0)
            {
                BodyData = packetBodyData;
            }
        }
                
        public static EFBinaryRequestInfo MakeNTFInConnectOrDisConnectClientPacket(bool isConnect, string sessionID)
        {
            var packet = new EFBinaryRequestInfo(null);
            
            if (isConnect)
            {
                packet.PacketID = (Int32)PACKETID.NTF_IN_CONNECT_CLIENT;
            }
            else
            {
                packet.PacketID = (Int32)PACKETID.NTF_IN_DISCONNECT_CLIENT;
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
        public ERROR_CODE Result;

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

}
