using System.Collections.Generic;

using DB;
using CSBaseLib;

namespace ChatServer;

public class PacketDistributor
{
    ConnectSessionManager SessionManager = new ConnectSessionManager();
    PacketProcessor CommonPacketProcessor = null;
    List<PacketProcessor> PacketProcessorList = new List<PacketProcessor>();

    DBProcessor DBWorker = new DBProcessor();

    RoomManager RoomMgr = new RoomManager();


    public ErrorCode Create(MainServer mainServer)
    {
        var roomThreadCount = MainServer.s_ServerOption.RoomThreadCount;
        
        Room.NetSendFunc = mainServer.SendData;

        SessionManager.CreateSession(ClientSession.s_MaxSessionCount);

        RoomMgr.CreateRooms();

        CommonPacketProcessor = new PacketProcessor();
        CommonPacketProcessor.CreateAndStart(true, null, mainServer, SessionManager);
                    
        for (int i = 0; i < roomThreadCount; ++i)
        {
            var packetProcess = new PacketProcessor();
            packetProcess.CreateAndStart(false, RoomMgr.GetRoomList(i), mainServer, SessionManager);
            PacketProcessorList.Add(packetProcess);
        }

        DBWorker.MainLogger = MainServer.s_MainLogger;
        var error = DBWorker.CreateAndStart(MainServer.s_ServerOption.DBWorkerThreadCount, DistributeDBJobResult, MainServer.s_ServerOption.RedisAddres);
        if (error != ErrorCode.None)
        {
            return error;
        }

        return ErrorCode.None;
    }

    public void Destory()
    {
        DBWorker.Destory();

        CommonPacketProcessor.Destory();

        PacketProcessorList.ForEach(preocess => preocess.Destory());
        PacketProcessorList.Clear();
    }

    public void Distribute(ServerPacketData requestPacket)
    {
        var packetId = (PacketId)requestPacket.PacketID;
        var sessionIndex = requestPacket.SessionIndex;
                    
        if(IsClientRequestPacket(packetId) == false)
        {
            MainServer.s_MainLogger.Debug("[Distribute] - 클라리언트의 요청 패킷이 아니다.");
            return; 
        }

        if(IsClientRequestCommonPacket(packetId))
        {
            DistributeCommon(true, requestPacket);
            return;
        }


        var roomNumber = SessionManager.GetRoomNumber(sessionIndex);
        if(DistributeRoomProcessor(true, false, roomNumber, requestPacket) == false)
        {
            return;
        }            
    }

    public void DistributeCommon(bool isClientPacket, ServerPacketData requestPacket)
    {
        CommonPacketProcessor.InsertMsg(isClientPacket, requestPacket);
    }

    public bool DistributeRoomProcessor(bool isClientPacket, bool isPreRoomEnter, int roomNumber, ServerPacketData requestPacket)
    {
        var sessionIndex = requestPacket.SessionIndex;
        var processor = PacketProcessorList.Find(x => x.관리중인_Room(roomNumber));
        if (processor != null)
        {
            if (isPreRoomEnter == false && SessionManager.IsStateRoom(sessionIndex) == false)
            {
                MainServer.s_MainLogger.Debug("[DistributeRoomProcessor] - 방에 입장하지 않은 유저 - 1");
                return false;
            }

            processor.InsertMsg(isClientPacket, requestPacket);
            return true;
        }

        MainServer.s_MainLogger.Debug("[DistributeRoomProcessor] - 방에 입장하지 않은 유저 - 2");
        return false;
    }


    public void DistributeDBJobRequest(DBQueue dbQueue)
    {
        DBWorker.InsertMsg(dbQueue);
    }

    public void DistributeDBJobResult(DBResultQueue resultData)
    {
        var sessionIndex = resultData.SessionIndex;

        var requestPacket = new ServerPacketData();
        requestPacket.Assign(resultData);

        DistributeCommon(false, requestPacket);            
    }

    bool IsClientRequestCommonPacket(PacketId packetId )
    {
        if ( packetId == PacketId.ReqLogin || packetId == PacketId.ReqRoomEnter)
        {
            return true;
        }

        return false;
    }

    bool IsClientRequestPacket(PacketId packetId)
    {
        return (PacketId.CsBegin < packetId && packetId < PacketId.CsEnd);
     }
}
