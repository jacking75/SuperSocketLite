using System;
using System.Collections.Generic;

using CommonLib;


namespace GateServer;

public class PKHandler
{
    Action<string, byte[]> SendPacketFunc;

    Dictionary<int, Action<ServerPacketData>> _packetHandlerMap = new ();


    public void Init(Action<string, byte[]> sendPacket)
    {
        SendPacketFunc = sendPacket;

        RegistPacketHandler();
    }


    public void RegistPacketHandler()
    {            
        //packetHandlerMap.Add((int)PACKETID.REQ_LOGIN, RequestLogin);

    }

    public void Process(ServerPacketData packet)
    {
        if (_packetHandlerMap.ContainsKey(packet.PacketID))
        {
            _packetHandlerMap[packet.PacketID](packet);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("세션 번호 {0}, PacketID {1}, 받은 데이터 크기: {2}", packet.SessionID, packet.PacketID, packet.BodyData.Length);
        }
    }


   

}
