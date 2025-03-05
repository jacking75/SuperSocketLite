using Google.Protobuf;

namespace TestProtocolBuffer;

internal class Program
{
	// protoc.exe -I=./ --csharp_out=./ ./packet_protocol.proto
    static void Main(string[] args)
    {
        Console.WriteLine("--- LoginRequest ---");
        PacketTest.LoginPacket();

        Console.WriteLine("--- MoveRequest ---");
        PacketTest.MovePacket();




    }

    static void TestLoginPacket()
    {
        var loginRequest = new LoginRequest
        {
            Header = new PacketHeader
            {
                TotalSize = UInt32.MaxValue, // 실제 크기는 직렬화 후 계산
                Id = 2,        // 패킷 ID
                Value = 7      // 패킷 타입
            },
            UserId = "jacking75",
            Password = "password"
        };

        byte[] serialized = loginRequest.ToByteArray();


        Console.WriteLine("일반적인 방법으로 직렬화를 풀어본다");
        LoginRequest request1 = LoginRequest.Parser.ParseFrom(serialized);
        Console.WriteLine($"UserId: {request1.UserId}, TotalSize: {request1.Header.TotalSize}");
        Console.WriteLine("");


        Console.WriteLine("직렬화한 데이터의 헤더 부분을 수동으로 변경한다");
        //주의: 수동으로 데이터를 바꾸는 경우 그 데이터는 절대 0인 상태에서 직렬화 되면 안된다. 0으로 하면 프로토버퍼가 최적화 해버린다. 0이 아닌 더미 값이을 꼭 넣어야 한다.
        uint totalSize = (uint)serialized.Length;
        Console.WriteLine($"serialized의 크기: {totalSize}");
        ProtocolBufferHeaderParser.WritePacketHeaderTotalSize(serialized, totalSize);

        LoginRequest request2 = LoginRequest.Parser.ParseFrom(serialized);
        Console.WriteLine($"UserId: {request2.UserId}, TotalSize: {request2.Header.TotalSize}");
        Console.WriteLine("");


        Console.WriteLine("헤더만 먼저 비직렬화 후 전체 비직렬화하기");
        PacketHeader header = ProtocolBufferHeaderParser.ParseHeaderOnly(serialized);
        Console.WriteLine($"TotalSize: {header.TotalSize}, Id: {header.Id}, Value: {header.Value}");

        LoginRequest request3 = LoginRequest.Parser.ParseFrom(serialized);
        Console.WriteLine($"UserId: {request2.UserId}, TotalSize: {request2.Header.TotalSize}");
        Console.WriteLine("");
    }

    static void TestMovePacket()
    {
        var requestPacket = new MoveRequest
        {
            Header = new PacketHeader
            {
                TotalSize = UInt32.MaxValue, // 실제 크기는 직렬화 후 계산
                Id = 3,        // 패킷 ID
                Value = 7      // 패킷 타입
            },
            
            PosX = 355,
            PosY = 123,
            PosZ = 987
        };

        byte[] serialized = requestPacket.ToByteArray();


        Console.WriteLine("일반적인 방법으로 직렬화를 풀어본다");
        MoveRequest request1 = MoveRequest.Parser.ParseFrom(serialized);
        Console.WriteLine($"PosX: {request1.PosX}, PosY: {request1.PosY}, TotalSize: {request1.Header.TotalSize}");
        Console.WriteLine("");


        Console.WriteLine("직렬화한 데이터의 헤더 부분을 수동으로 변경한다");
        uint totalSize = (uint)serialized.Length;
        Console.WriteLine($"serialized의 크기: {totalSize}");
        ProtocolBufferHeaderParser.WritePacketHeaderTotalSize(serialized, totalSize);

        MoveRequest request2 = MoveRequest.Parser.ParseFrom(serialized);
        Console.WriteLine($"PosX: {request1.PosX}, PosY: {request1.PosY}, TotalSize: {request1.Header.TotalSize}");
        Console.WriteLine("");


        Console.WriteLine("헤더만 먼저 비직렬화 후 전체 비직렬화하기");
        PacketHeader header = ProtocolBufferHeaderParser.ParseHeaderOnly(serialized);
        Console.WriteLine($"TotalSize: {header.TotalSize}, Id: {header.Id}, Value: {header.Value}");

        MoveRequest request3 = MoveRequest.Parser.ParseFrom(serialized);
        Console.WriteLine($"PosX: {request1.PosX}, PosY: {request1.PosY}, TotalSize: {request1.Header.TotalSize}");
        Console.WriteLine("");
    }
}



/*
message LoginRequest {
    PacketHeader header = 1;
    string userId = 2;
    string password = 3;
}

message PacketHeader {
    fixed32 totalSize = 1;
    int32 id = 2;
    int32 value = 3;
}


직렬화된 데이터 구조
[필드1 태그(1byte)][필드1 길이(varint)][필드1 데이터]
[필드2 태그(1byte)][필드2 길이(varint)][필드2 데이터]


실제 바이트 구조
public void ExplainSerializedData(byte[] serialized)
{
    Console.WriteLine("LoginRequest 직렬화 구조:");
    int pos = 0;

    // Header 필드 (필드번호 1)
    // 태그: 0x0A (필드번호 1, 와이어타입 2 Length-delimited)
    Console.WriteLine($"Position {pos}: 0x{serialized[pos]:X2} - Header 필드 태그");
    pos++;

    // Header 크기 (varint)
    byte headerSize = serialized[pos];
    Console.WriteLine($"Position {pos}: 0x{serialized[pos]:X2} - Header 크기");
    pos++;

    // PacketHeader 내부
    {
        // TotalSize 필드 (fixed32)
        // 태그: 0x0D (필드번호 1, 와이어타입 5 fixed32)
        Console.WriteLine($"Position {pos}: 0x{serialized[pos]:X2} - TotalSize 태그");
        pos++;
        
        // TotalSize 값 (4 bytes, little-endian)
        uint totalSize = BitConverter.ToUInt32(serialized, pos);
        Console.WriteLine($"Position {pos}-{pos+3}: {totalSize} (TotalSize 값)");
        pos += 4;

        // Id 필드
        // 태그: 0x10 (필드번호 2, 와이어타입 0 varint)
        Console.WriteLine($"Position {pos}: 0x{serialized[pos]:X2} - Id 태그");
        pos++;
        
        // Id 값
        Console.WriteLine($"Position {pos}: 0x{serialized[pos]:X2} - Id 값");
        pos++;

        // Value 필드
        // 태그: 0x18 (필드번호 3, 와이어타입 0 varint)
        Console.WriteLine($"Position {pos}: 0x{serialized[pos]:X2} - Value 태그");
        pos++;
        
        // Value 값
        Console.WriteLine($"Position {pos}: 0x{serialized[pos]:X2} - Value 값");
        pos++;
    }

    // UserId 필드 (필드번호 2)
    // 태그: 0x12 (필드번호 2, 와이어타입 2 Length-delimited)
    Console.WriteLine($"Position {pos}: 0x{serialized[pos]:X2} - UserId 필드 태그");
    pos++;
    
    // UserId 문자열 길이
    int userIdLength = serialized[pos];
    Console.WriteLine($"Position {pos}: {userIdLength} (UserId 길이)");
    pos++;
    
    // UserId 문자열 데이터
    string userId = System.Text.Encoding.UTF8.GetString(serialized, pos, userIdLength);
    Console.WriteLine($"Position {pos}-{pos+userIdLength-1}: {userId} (UserId 값)");
    pos += userIdLength;

    // Password 필드 구조도 UserId와 동일
    // ... 이하 생략
}



var loginRequest = new LoginRequest
{
    Header = new PacketHeader
    {
        TotalSize = 0,
        Id = 1,
        Value = 1
    },
    UserId = "testUser",
    Password = "testPass"
};

byte[] serialized = loginRequest.ToByteArray();
ExplainSerializedData(serialized);


따라서 TotalSize 값을 수정하려면:

Header 필드 태그(0x0A) 다음
Header 크기 다음
TotalSize 태그(0x0D) 다음 위치에 4바이트 값을 써야 합니다
즉, 정확한 TOTAL_SIZE_POSITION은 3이 되어야 합니다:

const int TOTAL_SIZE_POSITION = 3; // Header 태그(1) + Header 크기(1) + TotalSize 태그(1)

BinaryPrimitives.WriteUInt32LittleEndian(
    new Span<byte>(serialized, TOTAL_SIZE_POSITION, 4), 
    totalSize);
 */

/*
public void DeserializeExample(byte[] serialized)
{
    // 방법 1: Parser 사용
    LoginRequest request1 = LoginRequest.Parser.ParseFrom(serialized);
    Console.WriteLine($"UserId: {request1.UserId}, TotalSize: {request1.Header.TotalSize}");

    // 방법 2: 특정 범위만 파싱 (byte array의 일부분만 사용할 때)
    int offset = 0;
    int length = serialized.Length;
    LoginRequest request2 = LoginRequest.Parser.ParseFrom(serialized, offset, length);

    // 방법 3: ReadOnlySpan 사용
    LoginRequest request3 = LoginRequest.Parser.ParseFrom(new ReadOnlySpan<byte>(serialized));

    // 방법 4: Stream 사용
    using (MemoryStream stream = new MemoryStream(serialized))
    {
        LoginRequest request4 = LoginRequest.Parser.ParseFrom(stream);
    }

    // 방법 5: ByteString 사용
    ByteString byteString = ByteString.CopyFrom(serialized);
    LoginRequest request5 = LoginRequest.Parser.ParseFrom(byteString);
}

// 헤더만 먼저 파싱하고 싶을 때
public PacketHeader ParseHeaderOnly(byte[] data)
{
    const int HEADER_SIZE = 12; // 예시 크기, 실제 헤더 크기에 맞게 조정
    using (var stream = new MemoryStream(data, 0, HEADER_SIZE))
    {
        return PacketHeader.Parser.ParseFrom(stream);
    }
}

// 에러 처리를 포함한 버전
public LoginRequest DeserializeWithErrorHandling(byte[] serialized)
{
    try
    {
        return LoginRequest.Parser.ParseFrom(serialized);
    }
    catch (InvalidProtocolBufferException ex)
    {
        // 프로토콜 버퍼 파싱 에러
        Console.WriteLine($"Protocol buffer parsing error: {ex.Message}");
        throw;
    }
    catch (Exception ex)
    {
        // 기타 에러
        Console.WriteLine($"Deserialization error: {ex.Message}");
        throw;
    }
}

// 검증을 포함한 버전
public LoginRequest DeserializeWithValidation(byte[] serialized)
{
    if (serialized == null || serialized.Length == 0)
    {
        throw new ArgumentException("Serialized data is null or empty");
    }

    LoginRequest request = LoginRequest.Parser.ParseFrom(serialized);

    // 데이터 유효성 검사
    if (string.IsNullOrEmpty(request.UserId))
    {
        throw new InvalidDataException("UserId is required");
    }

    if (string.IsNullOrEmpty(request.Password))
    {
        throw new InvalidDataException("Password is required");
    }

    if (request.Header.TotalSize != serialized.Length)
    {
        throw new InvalidDataException("Invalid total size in header");
    }

    return request;
} 
 */ 