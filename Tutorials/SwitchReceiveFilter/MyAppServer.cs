using SuperSocketLite.SocketBase;
using SuperSocketLite.SocketBase.Protocol;


namespace SwitchReceiveFilter;

public class MyAppServer : AppServer
{
    public MyAppServer()
        : base(new DefaultReceiveFilterFactory<SwitchReceiveFilter, StringRequestInfo>())
    {

    }
}
