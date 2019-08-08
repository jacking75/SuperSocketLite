using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommonLib;


namespace GateServer
{
    public class PKHandler
    {
        Action<string, byte[]> SendPacket;

        Dictionary<int, Action<ServerPacketData>> packetHandlerMap = new Dictionary<int, Action<ServerPacketData>>();

        public void Init(Action<string, byte[]> sendPacket)
        {
            SendPacket = sendPacket;

            RegistPacketHandler();
        }


        public void RegistPacketHandler()
        {            
            //packetHandlerMap.Add((int)PACKETID.REQ_LOGIN, RequestLogin);

        }

        public void Process(ServerPacketData packet)
        {
            if (packetHandlerMap.ContainsKey(packet.PacketID))
            {
                packetHandlerMap[packet.PacketID](packet);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("세션 번호 {0}, PacketID {1}, 받은 데이터 크기: {2}", packet.SessionID, packet.PacketID, packet.BodyData.Length);
            }
        }

 
       

    }
}
