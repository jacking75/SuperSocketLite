using CSBaseLib;
using DB;

namespace ChatServer;

public class PKHandler
{
    protected MainServer _serverNetwork;
    protected ConnectSessionManager _sessionMgr;


    public void Init(MainServer serverNetwork, ConnectSessionManager sessionManager)
    {
        _serverNetwork = serverNetwork;
        _sessionMgr = sessionManager;
    }
            

    public bool RequestDBJob(PacketDistributor distributor, DBQueue dbQueue)
    {
        distributor.DistributeDBJobRequest(dbQueue);
        return true;
    }

    public DBQueue MakeDBQueue(PacketId packetID, string sessionID, int sessionIndex, byte[] jobDatas)
    {
        var dbQueue = new DBQueue()
        {
            PacketID    = packetID,
            SessionID   = sessionID,
            SessionIndex = sessionIndex, 
            Datas       = jobDatas
        };

        return dbQueue;
    }

    protected void SendInternalCommonPacket(ServerPacketData packetData)
    {
        _serverNetwork.GetPacketDistributor().DistributeCommon(false, packetData);
    }

    protected bool SendInternalRoomProcessor(bool isPreRoomEnter,  int roomNumber, ServerPacketData packetData)
    {
        return _serverNetwork.GetPacketDistributor().DistributeRoomProcessor(false, isPreRoomEnter, roomNumber, packetData);
    }
}
