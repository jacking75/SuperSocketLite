using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;
using CSBaseLib;


namespace DB;

public class DBQueue
{
    public PacketId PacketID;
    public int SessionIndex;
    public string SessionID;
    public byte[] Datas;
}

public class DBResultQueue
{
    public PacketId PacketID;
    public int SessionIndex;
    public string SessionID;
    public byte[] Datas;
}


[MessagePackObject]
public class DBReqLogin
{
    [Key(0)]
    public string UserID;

    [Key(1)]
    public string AuthToken;
}

[MessagePackObject]
public class DBResLogin
{
    [Key(0)]
    public string UserID;
    [Key(1)]
    public ErrorCode Result;
}
