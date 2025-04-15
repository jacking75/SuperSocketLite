using System;

using SuperSocketLite.Common;
using SuperSocketLite.SocketBase.Protocol;
using SuperSocketLite.SocketEngine.Protocol;


namespace GameServer;

public class EFBinaryRequestInfo : BinaryRequestInfo
{
    public UInt16 Size { get; private set; }
    public UInt16 PacketID { get; private set; }
    public SByte Type { get; private set; }

    public EFBinaryRequestInfo(UInt16 size, UInt16 packetID,  SByte type, byte[] body)
        : base(null, body)
    {
        this.Size = size;
        this.PacketID = packetID;
        this.Type = type;
    }
}

public class ReceiveFilter : FixedHeaderReceiveFilter<EFBinaryRequestInfo>
{
    public ReceiveFilter() : base(CSBaseLib.PacketDef.PACKET_HEADER_SIZE)
    {
    }

    protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(header, offset, CSBaseLib.PacketDef.PACKET_HEADER_SIZE);
        }

        var packetSize = BitConverter.ToInt16(header, offset);
        var bodySize = packetSize - CSBaseLib.PacketDef.PACKET_HEADER_SIZE;
        return bodySize;
    }

    protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] buffer, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(header.Array, 0, CSBaseLib.PacketDef.PACKET_HEADER_SIZE);

        return new EFBinaryRequestInfo(BitConverter.ToUInt16(header.Array, 0),
                                       BitConverter.ToUInt16(header.Array,  2),
                                       (SByte)header.Array[4],
                                       buffer.CloneRange(offset, length));
    }
}
