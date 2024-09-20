
using SuperSocket.SocketBase;


namespace GateServer;

public class ClientSession : AppSession<ClientSession, EFBinaryRequestInfo>
{
    public int SessionIndex { get; private set; } = -1;
                    
    public void SetSessionIndex(int sessionIndex)
    {
        SessionIndex = sessionIndex;
    }       
  
}
