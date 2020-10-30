using MessagePack; //https://github.com/neuecc/MessagePack-CSharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPGameServer
{
    public class PacketDef
    {
        public const Int16 MSGPACK_PACKET_HEADER_SIZE = 8;

        public const Int16 PACKET_HEADER_SIZE = 8;
        public const int MAX_USER_ID_BYTE_LENGTH = 16;
        public const int MAX_USER_PW_BYTE_LENGTH = 16;

        public const int INVALID_ROOM_NUMBER = -1;
    }

    public struct MsgPackPacketHeadInfo
    {
        const int PacketHeaderMsgPackStartPos = 3;
        public const int HeadSize = 8;

        public UInt16 TotalSize;
        public UInt16 Id;
        public byte Type;

        public static UInt16 GetTotalSize(byte[] data, int startPos)
        {
            return FastBinaryRead.UInt16(data, startPos + PacketHeaderMsgPackStartPos);
        }

        public static void WritePacketId(byte[] data, UInt16 packetId)
        {
            FastBinaryWrite.UInt16(data, PacketHeaderMsgPackStartPos + 2, packetId);
        }

        public void Read(byte[] headerData)
        {
            var pos = PacketHeaderMsgPackStartPos;

            TotalSize = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;

            Id = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;

            Type = headerData[pos];
            pos += 1;
        }

        public void Write(byte[] mqData)
        {
            var pos = PacketHeaderMsgPackStartPos;

            FastBinaryWrite.UInt16(mqData, pos, TotalSize);
            pos += 2;

            FastBinaryWrite.UInt16(mqData, pos, Id);
            pos += 2;

            mqData[pos] = Type;
            pos += 1;
        }
    }

    public class PacketToBytes
    {
        public static byte[] Make(PACKETID packetID, byte[] bodyData)
        {
            byte type = 0;
            var pktID = (Int16)packetID;
            Int16 bodyDataSize = 0;
            if (bodyData != null)
            {
                bodyDataSize = (Int16)bodyData.Length;
            }
            var packetSize = (Int16)(bodyDataSize + PacketDef.PACKET_HEADER_SIZE);
                        
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
            var bodySize = packetSize - PacketDef.PACKET_HEADER_SIZE;

            var packetBody = new byte[bodySize];
            Buffer.BlockCopy(recvData, PacketDef.PACKET_HEADER_SIZE, packetBody,  0, bodySize);

            return new Tuple<int, byte[]>(packetID, packetBody);
        }
    }


    [MessagePackObject]
    public class MsgPackPacketHead
    {
        [Key(0)]
        public Byte[] Head = new Byte[PacketDef.MSGPACK_PACKET_HEADER_SIZE];
    }


    // 로그인 요청
    [MessagePackObject]
    public class PKTReqLogin : MsgPackPacketHead
    {
        [Key(1)]
        public string UserID;
        [Key(2)]
        public string AuthToken;
    }

    [MessagePackObject]
    public class PKTResLogin : MsgPackPacketHead
    {
        [Key(1)]
        public short Result;
    }







    [MessagePackObject]
    public class PKNtfMustClose
    {
        [Key(0)]
        public short Result;
    }



    [MessagePackObject]
    public class PKTReqRoomEnter
    {
        [Key(0)]
        public int RoomNumber;
    }

    [MessagePackObject]
    public class PKTResRoomEnter
    {
        [Key(0)]
        public short Result;
    }

    [MessagePackObject]
    public class PKTNtfRoomUserList
    {
        [Key(0)]
        public List<string> UserIDList = new List<string>();
    }

    [MessagePackObject]
    public class PKTNtfRoomNewUser
    {
        [Key(0)]
        public string UserID;
    }


    [MessagePackObject]
    public class PKTReqRoomLeave
    {
    }

    [MessagePackObject]
    public class PKTResRoomLeave
    {
        [Key(0)]
        public short Result;
    }

    [MessagePackObject]
    public class PKTNtfRoomLeaveUser
    {
        [Key(0)]
        public string UserID;
    }


    [MessagePackObject]
    public class PKTReqRoomChat
    {
        [Key(0)]
        public string ChatMessage;
    }

    
    [MessagePackObject]
    public class PKTNtfRoomChat
    {
        [Key(0)]
        public string UserID;

        [Key(1)]
        public string ChatMessage;
    }
}
