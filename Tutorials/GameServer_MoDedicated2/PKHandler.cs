using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSBaseLib;


namespace GameServer;

public class PKHandler
{
    protected MainServer ServerNetwork;
    protected UserManager UserMgr = null;


    public void Init(MainServer serverNetwork, UserManager userMgr)
    {
        ServerNetwork = serverNetwork;
        UserMgr = userMgr;
    }            
            
}
