using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.Facility.Protocol;

namespace MultiPortServer
{
    public class EFBinaryRequestInfo : BinaryRequestInfo
    {
        public Int16 TotalSize { get; private set; }
        public Int16 PacketID { get; private set; }
        public SByte Value1 { get; private set; }

        public const int HEADERE_SIZE = 5;

        
        public EFBinaryRequestInfo(Int16 totalSize, Int16 packetID, SByte value1, byte[] body)
            : base(null, body)
        {
            this.TotalSize = totalSize;
            this.PacketID = packetID;
            this.Value1 = value1;
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

            var nBodySize = BitConverter.ToInt16(header, offset);
            return nBodySize - EFBinaryRequestInfo.HEADERE_SIZE;
        }

        protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header.Array, 0, EFBinaryRequestInfo.HEADERE_SIZE);

            return new EFBinaryRequestInfo(BitConverter.ToInt16(header.Array, 0),
                                           BitConverter.ToInt16(header.Array, 0 + 2),
                                           (SByte)header.Array[4], 
                                           bodyBuffer.CloneRange(offset, length));
        }
    }
}
