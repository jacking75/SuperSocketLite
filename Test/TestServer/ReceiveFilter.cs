using System;

using SuperSocketLite.Common;
using SuperSocketLite.SocketBase.Protocol;
using SuperSocketLite.SocketEngine.Protocol;


namespace EchoServer;

public class EFBinaryRequestInfo : BinaryRequestInfo
{
    /// <summary>
    /// 전체 크기
    /// </summary>
    public Int16 TotalSize { get; private set; }

    /// <summary>
    /// 패킷 ID
    /// </summary>
    public Int16 PacketID { get; private set; }

    /// <summary>
    /// 예약(더미)값 
    /// </summary>
    public SByte Value1 { get; private set; }

    /// <summary>
    /// 헤더 크기
    /// </summary>
    public const int HeaderSize = 5;

    /// <summary>
    /// EFBinaryRequestInfo 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="totalSize">전체 크기</param>
    /// <param name="packetID">패킷 ID</param>
    /// <param name="value1">값 1</param>
    /// <param name="body">바디</param>
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
    public ReceiveFilter() : base(EFBinaryRequestInfo.HeaderSize)
    {
    }

    protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(header, offset, 2);

        var packetTotalSize = BitConverter.ToInt16(header, offset);
        return packetTotalSize - EFBinaryRequestInfo.HeaderSize;
    }

    protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] buffer, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(header.Array, 0, EFBinaryRequestInfo.HeaderSize);
        }

        // 받은 데이터를 하나의 패킷을 다 만들면 offset의 위치는 언제나 할당 받은 buffer의 첫 위치이다.
        // 클라이언트에서 100바이트를 여러번 보내어도 offset은 13825(가정한 위치), 13825 가 된다.
        Console.WriteLine($"[ReceiveFilter.ResolveRequestInfo] offset:{offset}, length:{length}");

        return new EFBinaryRequestInfo(BitConverter.ToInt16(header.Array, 0),
                                       BitConverter.ToInt16(header.Array, 0 + 2),
                                       (SByte)header.Array[4],
                                       buffer.CloneRange(offset, length));
    }
}
