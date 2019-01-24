using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommonServerLib;
using CSBaseLib;

namespace ChatServer
{
    public class PacketDistributor
    {
        ConnectSessionManager SessionManager = new ConnectSessionManager();
        PacketProcessor CommonPacketProcessor = null;
        List<PacketProcessor> PacketProcessorList = new List<PacketProcessor>();

        DBProcessor DBWorker = new DBProcessor();

        RoomManager RoomMgr = new RoomManager();


        public ERROR_CODE Create(MainServer mainServer)
        {
            var roomThreadCount = ChatServerEnvironment.RoomThreadCount;
            
            Room.NetSendFunc = mainServer.SendData;

            SessionManager.CreateSession(ClientSession.MaxSessionCount);

            RoomMgr.CreateRooms();

            CommonPacketProcessor = new PacketProcessor();
            CommonPacketProcessor.CreateAndStart(true, null, mainServer, SessionManager);
                        
            for (int i = 0; i < roomThreadCount; ++i)
            {
                var packetProcess = new PacketProcessor();
                packetProcess.CreateAndStart(false, RoomMgr.GetRoomList(i), mainServer, SessionManager);
                PacketProcessorList.Add(packetProcess);
            }

            DBProcessor.LogFunc = MainServer.WriteLog;
            var error = DBWorker.CreateAndStart(ChatServerEnvironment.DBWorkerThreadCount, DistributeDBJobResult, ChatServerEnvironment.RedisAddress);
            if (error != ERROR_CODE.NONE)
            {
                return error;
            }

            return ERROR_CODE.NONE;
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
            var packetId = (PACKETID)requestPacket.PacketID;
            var sessionIndex = requestPacket.SessionIndex;
                        
            if(IsClientRequestPacket(packetId) == false)
            {
                MainServer.WriteLog("[Distribute] - 클라리언트의 요청 패킷이 아니다.", LOG_LEVEL.DEBUG);
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
                    MainServer.WriteLog("[DistributeRoomProcessor] - 방에 입장하지 않은 유저 - 1", LOG_LEVEL.DEBUG);
                    return false;
                }

                processor.InsertMsg(isClientPacket, requestPacket);
                return true;
            }

            MainServer.WriteLog("[DistributeRoomProcessor] - 방에 입장하지 않은 유저 - 2", LOG_LEVEL.DEBUG);
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

        bool IsClientRequestCommonPacket(PACKETID packetId )
        {
            if ( packetId == PACKETID.REQ_LOGIN || packetId == PACKETID.REQ_ROOM_ENTER)
            {
                return true;
            }

            return false;
        }

        bool IsClientRequestPacket(PACKETID packetId)
        {
            return (PACKETID.CS_BEGIN < packetId && packetId < PACKETID.CS_END);
         }
    }
}
