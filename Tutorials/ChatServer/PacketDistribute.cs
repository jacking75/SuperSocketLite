using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSBaseLib;

namespace ChatServer
{
    public class PacketDistributor
    {
        ConnectSessionManager SessionManager = new ConnectSessionManager();
        PacketProcessor MainPacketProcessor = null;
                
        RoomManager RoomMgr = new RoomManager();


        public ERROR_CODE Create(MainServer mainServer)
        {            
            Room.NetSendFunc = mainServer.SendData;

            SessionManager.CreateSession(ClientSession.MaxSessionCount);

            RoomMgr.CreateRooms();

            MainPacketProcessor = new PacketProcessor();
            MainPacketProcessor.CreateAndStart(RoomMgr.GetRoomsList(), mainServer, SessionManager);
                        
            return ERROR_CODE.NONE;
        }

        public void Destory()
        {
            MainPacketProcessor.Destory();           
        }

        public void Distribute(ServerPacketData requestPacket)
        {
            var packetId = (PACKETID)requestPacket.PacketID;
            var sessionIndex = requestPacket.SessionIndex;
                        
            if(IsClientRequestPacket(packetId) == false)
            {
                MainServer.MainLogger.Debug("[Distribute] - 클라리언트의 요청 패킷이 아니다.");
                return; 
            }

            MainPacketProcessor.InsertMsg(true, requestPacket);
        }

        public void DistributeCommon(bool isClientPacket, ServerPacketData requestPacket)
        {
            MainPacketProcessor.InsertMsg(isClientPacket, requestPacket);
        }                
 
        bool IsClientRequestCommonPacket(PACKETID packetId )
        {
            if ( packetId == PACKETID.REQ_LOGIN || packetId == PACKETID.REQ_ROOM_ENTER)
            {
                return true;
            }

            return false;
        }

        bool IsClientRequestPacket(PACKETID packetId)
        {
            return (PACKETID.CS_BEGIN < packetId && packetId < PACKETID.CS_END);
         }
    }
}
