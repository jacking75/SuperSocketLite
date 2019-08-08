using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;

namespace GateServer
{
    public class EFBinaryRequestInfo : BinaryRequestInfo
    {
        public Int16 Size { get; private set; }
        public Int16 PacketID { get; private set; }
        public SByte Type { get; private set; }

        public EFBinaryRequestInfo(Int16 size, Int16 packetID,  SByte type, byte[] body)
            : base(null, body)
        {
            this.Size = size;
            this.PacketID = packetID;
            this.Type = type;
        }
    }

    public class ReceiveFilter : FixedHeaderReceiveFilter<EFBinaryRequestInfo>
    {
        public ReceiveFilter() : base(CommonLib.PacketDef.PACKET_HEADER_SIZE)
        {
        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(header, offset, CommonLib.PacketDef.PACKET_HEADER_SIZE);
            }

            var packetSize = BitConverter.ToInt16(header, offset);
            var bodySize = packetSize - CommonLib.PacketDef.PACKET_HEADER_SIZE;
            return bodySize;
        }

        protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header.Array, 0, CommonLib.PacketDef.PACKET_HEADER_SIZE);

            return new EFBinaryRequestInfo(BitConverter.ToInt16(header.Array, 0),
                                           BitConverter.ToInt16(header.Array,  2),
                                           (SByte)header.Array[4],
                                           bodyBuffer.CloneRange(offset, length));
        }
    }
}
