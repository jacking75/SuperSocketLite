using System;

using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;

namespace GameServer_MemoryPack;

/// <summary>
/// 이진 요청 정보 클래스
/// 패킷의 헤더와 보디에 해당하는 부분을 나타냅니다.
/// </summary>
public class PacketRequestInfo : BinaryRequestInfo
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
    public PacketRequestInfo(byte[] packetData)
        : base(null, packetData)
    {
        Data = packetData;
    }
}

/// <summary>
/// 수신 필터 클래스
/// </summary>
public class PacketReceiveFilter : FixedHeaderReceiveFilter<PacketRequestInfo>
{
    /// <summary>
    /// ReceiveFilter 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public PacketReceiveFilter() : base(PacketRequestInfo.HeaderSize)
    {
    }

    /// <summary>
    /// 헤더에서 바디 길이를 가져옵니다.
    /// </summary>
    /// <param name="header">헤더</param>
    /// <param name="offset">오프셋</param>
    /// <param name="length">길이</param>
    /// <returns>바디 길이</returns>
    protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(header, offset, 2);
        }

        var totalSize = BitConverter.ToUInt16(header, offset + PacketRequestInfo.PacketHeaderMemorypackStartPos);
        return totalSize - PacketRequestInfo.HeaderSize;
    }

    /// <summary>
    /// 요청 정보를 해결합니다.
    /// </summary>
    /// <param name="header">헤더</param>
    /// <param name="buffer">바디 버퍼</param>
    /// <param name="offset">오프셋. receive 버퍼 내의 오프셋으로 패킷의 보디의 시작 지점을 가리킨다</param>
    /// <param name="length">길이. 패킷 바디의 크기</param>
    /// <returns>해결된 요청 정보</returns>
    protected override PacketRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] buffer, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(header.Array, 0, PacketRequestInfo.HeaderSize);
        }

        // body 데이터가 있는 경우
        if (length > 0)
        {
            if (offset >= PacketRequestInfo.HeaderSize)
            {
                var packetStartPos = offset - PacketRequestInfo.HeaderSize;
                var packetSize = length + PacketRequestInfo.HeaderSize;

                return new PacketRequestInfo(buffer.CloneRange(packetStartPos, packetSize));
            }
            else
            {
                // offset 이 헤더 크기보다 작으므로 헤더와 보디를 직접 합쳐야 한다.
                var packetData = new Byte[length + PacketRequestInfo.HeaderSize];
                header.CopyTo(packetData, 0);
                Array.Copy(buffer, offset, packetData, PacketRequestInfo.HeaderSize, length);

                return new PacketRequestInfo(packetData);
            }
        }

        // body 데이터가 없는 경우
        return new PacketRequestInfo(header.CloneRange(header.Offset, header.Count));
    }
}
