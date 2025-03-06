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

        Console.WriteLine("--- SendMailRequest ---");
        PacketTest.SendMailPacket();
    }

    
}