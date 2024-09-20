namespace GameServer;

public class PKHandler
{
    protected MainServer _serverNetwork;
    protected UserManager _userMgr = null;


    public void Init(MainServer serverNetwork, UserManager userMgr)
    {
        _serverNetwork = serverNetwork;
        _userMgr = userMgr;
    }            
            
}
