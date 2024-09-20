using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BinaryPacketServer;

public class PacketData
{
    public NetworkSession Session;
    public EFBinaryRequestInfo ReqInfo;
}

public enum PACKETID : int
{
    ReqEcho = 1,
}

public class CommonHandler
{
    public void RequestEcho(NetworkSession session, EFBinaryRequestInfo requestInfo)
    {
        List<byte> dataSource = new List<byte>();
        dataSource.AddRange(BitConverter.GetBytes((int)PACKETID.ReqEcho));
        dataSource.AddRange(BitConverter.GetBytes(requestInfo.Body.Length));
        dataSource.AddRange(requestInfo.Body);

        session.Send(dataSource.ToArray(), 0, dataSource.Count);
    }
}

public class PktEcho
{
    public string Message;
}
