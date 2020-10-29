using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPGameServer
{
    public class PKHandler
    {
        public static Func<string, byte[], bool> NetSendFunc;
        public static Action<ServerPacketData> DistributePacket;

        protected UserManager UserMgr = null;


        public void Init(UserManager userMgr)
        {
            UserMgr = userMgr;
        }            
                
    }
}
