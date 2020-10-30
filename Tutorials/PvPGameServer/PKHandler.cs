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
                
        public void WriteHeaderInfo(PACKETID packetId, byte[] packetData)
        {
            var header = new MsgPackPacketHeadInfo();
            header.TotalSize = (UInt16)packetData.Length;
            header.Id = (UInt16)packetId;
            header.Type = 0;
            header.Write(packetData);
        }
    }
}
