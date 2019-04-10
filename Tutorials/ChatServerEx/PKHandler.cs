using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSBaseLib;
using DB;

namespace ChatServer
{
    public class PKHandler
    {
        protected MainServer ServerNetwork;
        protected ConnectSessionManager SessionManager;


        public void Init(MainServer serverNetwork, ConnectSessionManager sessionManager)
        {
            ServerNetwork = serverNetwork;
            SessionManager = sessionManager;
        }
                

        public bool RequestDBJob(PacketDistributor distributor, DBQueue dbQueue)
        {
            distributor.DistributeDBJobRequest(dbQueue);
            return true;
        }

        public DBQueue MakeDBQueue(PACKETID packetID, string sessionID, int sessionIndex, byte[] jobDatas)
        {
            var dbQueue = new DBQueue()
            {
                PacketID    = packetID,
                SessionID   = sessionID,
                SessionIndex = sessionIndex, 
                Datas       = jobDatas
            };

            return dbQueue;
        }

        protected void SendInternalCommonPacket(ServerPacketData packetData)
        {
            ServerNetwork.GetPacketDistributor().DistributeCommon(false, packetData);
        }

        protected bool SendInternalRoomProcessor(bool isPreRoomEnter,  int roomNumber, ServerPacketData packetData)
        {
            return ServerNetwork.GetPacketDistributor().DistributeRoomProcessor(false, isPreRoomEnter, roomNumber, packetData);
        }
    }
}
