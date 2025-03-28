using System;
using System.Collections.Generic;


namespace GameServer_01_GenericHost;

public class PacketData
{
    public NetworkSession session;
    public PacketRequestInfo reqInfo;
}

public enum PacketId : int
{
    ReqEcho = 101,
}

public class CommonHandler
{
    public void RequestEcho(NetworkSession session, PacketRequestInfo requestInfo)
    {
        var totalSize = (Int16)(requestInfo.Body.Length + PacketRequestInfo.HeaderSize);

        List<byte> dataSource = new List<byte>();
        dataSource.AddRange(BitConverter.GetBytes(totalSize));
        dataSource.AddRange(BitConverter.GetBytes((Int16)PacketId.ReqEcho));
        dataSource.AddRange(new byte[1]);
        dataSource.AddRange(requestInfo.Body);

        session.Send(dataSource.ToArray(), 0, dataSource.Count);
    }
}

public class PkEcho
{
    public string msg;
}
