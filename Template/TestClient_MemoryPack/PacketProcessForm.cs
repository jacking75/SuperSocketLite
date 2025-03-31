using MemoryPack;
using System;
using System.Collections.Generic;


namespace csharp_test_client;

public partial class mainForm
{
    Dictionary<PacketId, Action<byte[]>> PacketFuncDic = new ();

    void SetPacketHandler()
    {
        PacketFuncDic.Add(PacketId.ReqResTestEcho, PacketProcess_Echo);
    
    }

    void PacketProcess(byte[] packet)
    {
        var header = new MemoryPackPacketHeader();
        header.Read(packet);

        var packetType = (PacketId)header.Id;
        //DevLog.Write("Packet Error:  PacketID:{packet.PacketID.ToString()},  Error: {(ERROR_CODE)packet.Result}");
        //DevLog.Write("RawPacket: " + packet.PacketID.ToString() + ", " + PacketDump.Bytes(packet.BodyData));

        if (PacketFuncDic.ContainsKey(packetType))
        {
            PacketFuncDic[packetType](packet);
        }
        else
        {
            DevLog.Write("Unknown Packet Id: " + packetType);
        }
    }

    void PacketProcess_Echo(byte[] packet)
    {
        var reqData = MemoryPackSerializer.Deserialize<PKTReqResEcho>(packet);
        DevLog.Write($"Echo 받음:  {reqData.DummyData}");
    }

    
}
