using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonServerLib
{
    public class InnerMessageHostProgram
    {
        static System.Collections.Concurrent.ConcurrentQueue<InnerMsg> msgQueue = new System.Collections.Concurrent.ConcurrentQueue<InnerMsg>();

        static public bool GetMsg(out InnerMsg msg)
        {
            return msgQueue.TryDequeue(out msg);
        }

        public static void ServerStart(int ServerID, int Port)
        {
            var msg = new InnerMsg() { Type = InnerMsgType.SERVER_START };
            msg.Value1 = string.Format("{0}_{1}", ServerID, Port);

            msgQueue.Enqueue(msg);
        }

        public static void CreateComponent()
        {
            var msg = new InnerMsg() { Type = InnerMsgType.CREATE_COMPONENT };
            msgQueue.Enqueue(msg);
        }

        public static void CurrentUserCount(int count)
        {
            var msg = new InnerMsg() { Type = InnerMsgType.CURRENT_CONNECT_COUNT };
            msg.Value1 = count.ToString();
            msgQueue.Enqueue(msg);
        }
    }


    public enum InnerMsgType
    {
        SERVER_START            = 1,
        CREATE_COMPONENT        = 2,
        CURRENT_CONNECT_COUNT   = 3,
    }

    public class InnerMsg
    {
        public InnerMsgType Type;
        public string SessionID;
        public string Value1;
        public string Value2;
    }
}
