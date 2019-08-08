using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommonLib;

namespace GateServer
{
    public class ClientSessionManager
    {
        static public int MaxSessionCount { get; private set; } = 0;
        static ConcurrentBag<int> IndexPool = new ConcurrentBag<int>();

        ConcurrentDictionary<int, ClientSession> SessionMap = new ConcurrentDictionary<int, ClientSession>();


        public void Init(int maxCount)
        {
            for (int i = 0; i < maxCount; ++i)
            {
                IndexPool.Add(i);
            }

            MaxSessionCount = maxCount;
        }
         
        public bool NewSession(ClientSession session)
        {
            var index = PopIndex();
            if(index == -1)
            {
                return false;
            }

            session.SetSessionIndex(index);
            SessionMap.TryAdd(index, session);
            return true;
        }

        public void ColesdSession(ClientSession session)
        {
            PushIndex(session.SessionIndex);
            SessionMap.TryRemove(session.SessionIndex, out var _);
        }

        int PopIndex()
        {
            if (IndexPool.TryTake(out var result))
            {
                return result;
            }

            return -1;
        }

        void PushIndex(int index)
        {
            if (index >= 0)
            {
                IndexPool.Add(index);
            }
        }
                
        


    }


    
}
