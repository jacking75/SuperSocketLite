using System;
using System.Collections.Generic;

namespace EchoServerEx;

public class PacketData
{
    public NetworkSession Session;
    public EFBinaryRequestInfo ReqInfo;
}

public enum PacketId : int
{
    ReqEcho = 101,
}

public class CommonHandler
{
    public void RequestEcho(NetworkSession session, EFBinaryRequestInfo requestInfo)
    {
        var totalSize = (Int16)(requestInfo.Body.Length + EFBinaryRequestInfo.HeaderSize);

        List<byte> dataSource =
        [
            .. BitConverter.GetBytes(totalSize),
            .. BitConverter.GetBytes((Int16)PacketId.ReqEcho),
            .. new byte[1],
            .. requestInfo.Body,
        ];

        session.Send(dataSource.ToArray(), 0, dataSource.Count);
    }
}

public class PK_ECHO
{
    public string Message;
}
