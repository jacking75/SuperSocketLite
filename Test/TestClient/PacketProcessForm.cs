using System;
using System.Collections.Generic;


namespace csharp_test_client;

public partial class mainForm
{
    Dictionary<PACKET_ID, Action<byte[]>> PacketFuncDic = new Dictionary<PACKET_ID, Action<byte[]>>();

    void SetPacketHandler()
    {
        PacketFuncDic.Add(PACKET_ID.PACKET_ID_ECHO, PacketProcess_Echo);
    
    }

    void PacketProcess(PacketData packet)
    {
        var packetType = (PACKET_ID)packet.PacketID;
        //DevLog.Write("Packet Error:  PacketID:{packet.PacketID.ToString()},  Error: {(ERROR_CODE)packet.Result}");
        //DevLog.Write("RawPacket: " + packet.PacketID.ToString() + ", " + PacketDump.Bytes(packet.BodyData));

        if (PacketFuncDic.ContainsKey(packetType))
        {
            PacketFuncDic[packetType](packet.BodyData);
        }
        else
        {
            DevLog.Write("Unknown Packet Id: " + packet.PacketID.ToString());
        }         
    }

    void PacketProcess_Echo(byte[] bodyData)
    {
        DevLog.Write($"Echo 받음:  {bodyData.Length}");
    }

    
}
