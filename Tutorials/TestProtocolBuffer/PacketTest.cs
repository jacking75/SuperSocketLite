using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProtocolBuffer;

public class PacketTest
{
    public static void LoginPacket()
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

    public static void MovePacket()
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
        Console.WriteLine($"PosX: {request2.PosX}, PosY: {request2.PosY}, TotalSize: {request2.Header.TotalSize}");
        Console.WriteLine("");


        Console.WriteLine("헤더만 먼저 비직렬화 후 전체 비직렬화하기");
        PacketHeader header = ProtocolBufferHeaderParser.ParseHeaderOnly(serialized);
        Console.WriteLine($"TotalSize: {header.TotalSize}, Id: {header.Id}, Value: {header.Value}");

        MoveRequest request3 = MoveRequest.Parser.ParseFrom(serialized);
        Console.WriteLine($"PosX: {request3.PosX}, PosY: {request3.PosY}, TotalSize: {request3.Header.TotalSize}");
        Console.WriteLine("");
    }
}
