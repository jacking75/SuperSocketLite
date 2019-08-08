using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SuperSocket.SocketBase;

namespace GateServer
{
    public class ClientSession : AppSession<ClientSession, EFBinaryRequestInfo>
    {
        public int SessionIndex { get; private set; } = -1;
                        
        public void SetSessionIndex(int sessionIndex)
        {
            SessionIndex = sessionIndex;
        }       
      
    }
}
