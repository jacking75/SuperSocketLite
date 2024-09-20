using System;

using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;


namespace PvPGameServer;


/// <summary>
/// 메모리 팩으로 직렬화된 이진 요청 정보를 나타내는 클래스입니다.
/// </summary>
public class MemoryPackBinaryRequestInfo : BinaryRequestInfo
{
    /// <summary>
    /// 세션 ID를 나타냅니다.
    /// </summary>
    public string SessionID;

    /// <summary>
    /// 패킷의 헤더와 바디 전체를 나타내는 바이트 배열입니다.
    /// </summary>
    public byte[] Data;

    /// <summary>
    /// 패킷 헤더의 메모리 팩 시작 위치입니다.
    /// </summary>
    public const int PacketHeaderMemorypackStartPos = 1;

    /// <summary>
    /// 패킷 헤더의 크기입니다. 5는 실제 헤더의 크기이다
    /// </summary>
    public const int HeaderSize = 5 + PacketHeaderMemorypackStartPos;

    /// <summary>
    /// MemoryPackBinaryRequestInfo 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="packetData">패킷 데이터</param>
    public MemoryPackBinaryRequestInfo(byte[] packetData)
        : base(null, packetData)
    {
        Data = packetData;
    }
}

/// <summary>
/// MemoryPackBinaryRequestInfo를 사용하는 고정 헤더 수신 필터 클래스입니다.
/// </summary>
public class ReceiveFilter : FixedHeaderReceiveFilter<MemoryPackBinaryRequestInfo>
{
    /// <summary>
    /// ReceiveFilter 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public ReceiveFilter() : base(MemoryPackBinaryRequestInfo.HeaderSize)
    {
    }

    /// <summary>
    /// 헤더에서 바디 길이를 가져옵니다.
    /// </summary>
    /// <param name="header">헤더 데이터</param>
    /// <param name="offset">오프셋</param>
    /// <param name="length">길이</param>
    /// <returns>바디 길이</returns>
    protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(header, offset, 2);
        }

        var totalSize = BitConverter.ToUInt16(header, offset + MemoryPackBinaryRequestInfo.PacketHeaderMemorypackStartPos);
        return totalSize - MemoryPackBinaryRequestInfo.HeaderSize;
    }

    /// <summary>
    /// 요청 정보를 해결하여 MemoryPackBinaryRequestInfo 인스턴스를 반환합니다.
    /// </summary>
    /// <param name="header">헤더 데이터 세그먼트</param>
    /// <param name="readBuffer">읽기 버퍼</param>
    /// <param name="offset">오프셋</param>
    /// <param name="length">길이</param>
    /// <returns>MemoryPackBinaryRequestInfo 인스턴스</returns>
    protected override MemoryPackBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] readBuffer, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(header.Array, 0, MemoryPackBinaryRequestInfo.HeaderSize);
        }

        // body 데이터가 있는 경우
        if (length > 0)
        {
            if (offset >= MemoryPackBinaryRequestInfo.HeaderSize)
            {
                var packetStartPos = offset - MemoryPackBinaryRequestInfo.HeaderSize;
                var packetSize = length + MemoryPackBinaryRequestInfo.HeaderSize;

                return new MemoryPackBinaryRequestInfo(readBuffer.CloneRange(packetStartPos, packetSize));
            }
            else
            {
                // offset 이 헤더 크기보다 작으므로 헤더와 보디를 직접 합쳐야 한다.
                var packetData = new Byte[length + MemoryPackBinaryRequestInfo.HeaderSize];
                header.CopyTo(packetData, 0);
                Array.Copy(readBuffer, offset, packetData, MemoryPackBinaryRequestInfo.HeaderSize, length);

                return new MemoryPackBinaryRequestInfo(packetData);
            }
        }

        // body 데이터가 없는 경우
        return new MemoryPackBinaryRequestInfo(header.CloneRange(header.Offset, header.Count));
    }
}
