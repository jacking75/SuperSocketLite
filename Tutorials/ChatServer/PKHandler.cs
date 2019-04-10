using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSBaseLib;


namespace ChatServer
{
    public class PKHandler
    {
        protected MainServer ServerNetwork;
        protected ConnectSessionManager SessionManager;
        protected UserManager UserMgr = null;


        public void Init(MainServer serverNetwork, ConnectSessionManager sessionManager, UserManager userMgr)
        {
            ServerNetwork = serverNetwork;
            SessionManager = sessionManager;
            UserMgr = userMgr;
        }                
        
        protected void SendInternalCommonPacket(ServerPacketData packetData)
        {
            ServerNetwork.GetPacketDistributor().DistributeCommon(false, packetData);
        }        
    }
}
