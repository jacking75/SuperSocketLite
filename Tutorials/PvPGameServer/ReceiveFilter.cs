using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;

namespace PvPGameServer
{
    //TODO msgpack 일체형으로 예제 코드 추가
    public class EFBinaryRequestInfo : BinaryRequestInfo
    {
        //public UInt16 TotalSize;
        //public UInt16 PacketID;
        //public Byte Type;

        public string SessionID;
        public byte[] Data;

        public const int PACKET_HEADER_MSGPACK_START_POS = 3;
        public const int HEADERE_SIZE = 5 + PACKET_HEADER_MSGPACK_START_POS;
                
        /*public EFBinaryRequestInfo(UInt16 totalSize, UInt16 packetID, Byte type, byte[] body)
            : base(null, body)
        {
            this.TotalSize = totalSize;
            this.PacketID = packetID;
            this.Type = type;
        }*/
        public EFBinaryRequestInfo(byte[] packetData)
            : base(null, packetData)
        {
            Data = packetData;
        }

    }

    public class ReceiveFilter : FixedHeaderReceiveFilter<EFBinaryRequestInfo>
    {
        public ReceiveFilter() : base(EFBinaryRequestInfo.HEADERE_SIZE)
        {
        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header, offset, 2);

            var totalSize = BitConverter.ToUInt16(header, offset + EFBinaryRequestInfo.PACKET_HEADER_MSGPACK_START_POS);
            return totalSize - EFBinaryRequestInfo.HEADERE_SIZE;
        }

        protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header.Array, 0, EFBinaryRequestInfo.HEADERE_SIZE);

            var packetStartPos = offset - EFBinaryRequestInfo.HEADERE_SIZE;
            var packetSize = length + EFBinaryRequestInfo.HEADERE_SIZE;
            return new EFBinaryRequestInfo(bodyBuffer.CloneRange(packetStartPos, packetSize));
            /*return new EFBinaryRequestInfo(BitConverter.ToUInt16(header.Array, pos),
                                           BitConverter.ToUInt16(header.Array, pos + 2),
                                           (Byte)header.Array[pos+4], 
                                           bodyBuffer.CloneRange(offset, length));*/
        }
    }
}
