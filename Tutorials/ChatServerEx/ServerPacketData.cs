using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DB;
using CSBaseLib;
using MessagePack;

namespace ChatServer
{
    public class RawPacketData
    {
        public short Size;
        public short PacketID;
        public sbyte Type;
        public byte[] Body;
    }

    public class ServerPacketData
    {
        public Int16 PacketSize;
        public string SessionID; 
        public int SessionIndex;
        public Int16 PacketID;        
        public SByte Type;
        public byte[] BodyData;
                
        
        public void Assign(string sessionID, int sessionIndex, Int16 packetID, byte[] packetBodyData)
        {
            SessionIndex = sessionIndex;
            SessionID = sessionID;

            PacketID = packetID;
            
            if (packetBodyData.Length > 0)
            {
                BodyData = packetBodyData;
            }
        }

        public void Assign(DBResultQueue DBResult)
        {
            SessionIndex = DBResult.SessionIndex;
            SessionID = DBResult.SessionID;

            PacketID = (short)DBResult.PacketID;
            BodyData = DBResult.Datas;
        }

        public static ServerPacketData MakeNTFInConnectOrDisConnectClientPacket(bool isConnect, string sessionID, int sessionIndex)
        {
            var packet = new ServerPacketData();
            
            if (isConnect)
            {
                packet.PacketID = (Int32)PACKETID.NTF_IN_CONNECT_CLIENT;
            }
            else
            {
                packet.PacketID = (Int32)PACKETID.NTF_IN_DISCONNECT_CLIENT;
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
