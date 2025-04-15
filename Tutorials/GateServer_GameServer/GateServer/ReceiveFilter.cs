using System;

using SuperSocketLite.Common;
using SuperSocketLite.SocketBase.Protocol;
using SuperSocketLite.SocketEngine.Protocol;


namespace GateServer;

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
    public ReceiveFilter() : base(CommonLib.PacketDef.PacketHeaderSize)
    {
    }

    protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(header, offset, CommonLib.PacketDef.PacketHeaderSize);
        }

        var packetSize = BitConverter.ToInt16(header, offset);
        var bodySize = packetSize - CommonLib.PacketDef.PacketHeaderSize;
        return bodySize;
    }

    protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] buffer, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(header.Array, 0, CommonLib.PacketDef.PacketHeaderSize);

        return new EFBinaryRequestInfo(BitConverter.ToInt16(header.Array, 0),
                                       BitConverter.ToInt16(header.Array,  2),
                                       (SByte)header.Array[4],
                                       buffer.CloneRange(offset, length));
    }
}
