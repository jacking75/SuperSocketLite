using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;

using CSBaseLib;
using CommonServerLib;


namespace ChatServer
{
    public class PKHCommon : PKHandler
    {
        UserManager UserMgr = new UserManager();

        public void SetConfig(int maxUserCount)
        {
            UserMgr.Init(maxUserCount);
        }

        public void RegistPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerMap)
        {            
            packetHandlerMap.Add((int)PACKETID.NTF_IN_CONNECT_CLIENT, NotifyInConnectClient);
            packetHandlerMap.Add((int)PACKETID.NTF_IN_DISCONNECT_CLIENT, NotifyInDisConnectClient);

            packetHandlerMap.Add((int)PACKETID.REQ_LOGIN, RequestLogin);
            packetHandlerMap.Add((int)PACKETID.REQ_ROOM_ENTER, RequestRoomEnter);            

            packetHandlerMap.Add((int)PACKETID.RES_IN_ROOM_ENTER, ResponseRoomEnterInternal);
                        
            packetHandlerMap.Add((int)PACKETID.RES_DB_LOGIN, ResponseLoginFromDB);
        }

        public void NotifyInConnectClient(ServerPacketData requestData)
        {
            InnerMessageHostProgram.CurrentUserCount(ServerNetwork.SessionCount);

            
        }

        public void NotifyInDisConnectClient(ServerPacketData requestData)
        {
            var sessionIndex = requestData.SessionIndex;
            var roomNum = SessionManager.GetRoomNumber(sessionIndex);
            var user = UserMgr.GetUser(sessionIndex);

            if (roomNum != PacketDef.INVALID_ROOM_NUMBER && user != null)
            {                
                SendInternalRoomLeavePacket(roomNum, user.ID());
            }

            if (user != null)
            {
                UserMgr.RemoveUser(sessionIndex);
            }

            SessionManager.SetClear(sessionIndex);
            InnerMessageHostProgram.CurrentUserCount(ServerNetwork.SessionCount);
        }


        public void RequestLogin(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            MainServer.WriteLog("로그인 요청 받음", LOG_LEVEL.DEBUG);

            try
            {
                if( SessionManager.EnableReuqestLogin(sessionIndex) == false)
                {
                    ResponseLoginToClient(ERROR_CODE.LOGIN_ALREADY_WORKING, packetData.SessionID);
                    return;
                }
                                
                var reqData = MessagePackSerializer.Deserialize< PKTReqLogin>(packetData.BodyData);

                // 세션의 상태를 바꾼다
                SessionManager.SetPreLogin(sessionIndex);
                
                // DB 작업 의뢰한다.
                var dbReqLogin = new DBReqLogin()
                {
                    UserID = reqData.UserID,
                    AuthToken = reqData.AuthToken
                };
                var jobDatas = MessagePackSerializer.Serialize(dbReqLogin);
                
                var dbQueue = MakeDBQueue(PACKETID.REQ_DB_LOGIN, sessionID, sessionIndex, jobDatas);
                RequestDBJob(ServerNetwork.GetPacketDistributor(), dbQueue);

                MainServer.WriteLog("DB에 로그인 요청 보냄", LOG_LEVEL.DEBUG);
            }
            catch(Exception ex)
            {
                // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
                MainServer.WriteLog(ex.ToString(), LOG_LEVEL.DEBUG);
            }
        }

        public void ResponseLoginFromDB(ServerPacketData packetData)
        {
            var sessionIndex = packetData.SessionIndex;
            MainServer.WriteLog("DB에서 로그인 답변 받음", LOG_LEVEL.DEBUG);

            try
            {
                var resData = MessagePackSerializer.Deserialize<DBResLogin>(packetData.BodyData);
                
                // DB 처리 성공/실패에 대한 처리를 한다.
                var errorCode = ERROR_CODE.NONE;

                if (resData.Result != ERROR_CODE.NONE)
                {
                    errorCode = ERROR_CODE.LOGIN_INVALID_AUTHTOKEN;
                    ResponseLoginToClient(errorCode, packetData.SessionID);
                    SessionManager.SetStateNone(sessionIndex);
                    return;
                }

                errorCode = UserMgr.AddUser(resData.UserID, packetData.SessionID, packetData.SessionIndex);
                if(errorCode != ERROR_CODE.NONE)
                {                    
                    ResponseLoginToClient(errorCode, packetData.SessionID);
                  
                    if(errorCode == ERROR_CODE.LOGIN_FULL_USER_COUNT)
                    {
                        NotifyMustCloseToClient(ERROR_CODE.LOGIN_FULL_USER_COUNT, packetData.SessionID);
                        SessionManager.SetDisable(sessionIndex);
                    }
                    else
                    {
                        SessionManager.SetStateNone(sessionIndex);
                    }

                    return;
                }

                SessionManager.SetLogin(sessionIndex, resData.UserID);

                ResponseLoginToClient(errorCode, packetData.SessionID);

                MainServer.WriteLog("로그인 요청 답변 보냄", LOG_LEVEL.DEBUG);
            }
            catch (Exception ex)
            {
                // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
                MainServer.WriteLog(ex.ToString(), LOG_LEVEL.DEBUG);
            }
        }

        public void ResponseLoginToClient(ERROR_CODE errorCode, string sessionID)
        {
            var resLogin = new PKTResLogin()
            {
                Result = (short)errorCode
            };

            var bodyData = MessagePackSerializer.Serialize(resLogin);
            var sendData = PacketToBytes.Make(PACKETID.RES_LOGIN, bodyData);

            ServerNetwork.SendData(sessionID, sendData);
        }

        public void NotifyMustCloseToClient(ERROR_CODE errorCode, string sessionID)
        {
            var resLogin = new PKNtfMustClose()
            {
                Result = (short)errorCode
            };

            var bodyData = MessagePackSerializer.Serialize(resLogin);
            var sendData = PacketToBytes.Make(PACKETID.NTF_MUST_CLOSE, bodyData);

            ServerNetwork.SendData(sessionID, sendData);
        }


        public void RequestRoomEnter(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            MainServer.WriteLog("RequestRoomEnter", LOG_LEVEL.DEBUG);

            try
            {
                var user = UserMgr.GetUser(sessionIndex);
                
                if(user == null || user.IsConfirm(sessionID) == false)
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_USER, sessionID);
                    return;
                }

                if (SessionManager.EnableReuqestEnterRoom(sessionIndex) == false)
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_STATE, sessionID);
                    return;
                }

                var reqData = MessagePackSerializer.Deserialize<PKTReqRoomEnter>(packetData.BodyData);

                var internalRoomEnter = MakeInternalRoomEnterPacket(reqData.RoomNumber, user.ID(), sessionID, sessionIndex);
                if (SendInternalRoomProcessor(true, reqData.RoomNumber, internalRoomEnter) == false)
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_ERROR_SYSTEM, sessionID);
                    return;
                }

                SessionManager.SetPreRoomEnter(sessionIndex, reqData.RoomNumber);

                MainServer.WriteLog("패킷 분배", LOG_LEVEL.DEBUG);
            }
            catch (Exception ex)
            {
                MainServer.WriteLog(ex.ToString(), LOG_LEVEL.DEBUG);
            }
        }

        void ResponseEnterRoomToClient(ERROR_CODE errorCode,  string sessionID)
        {
            var resRoomEnter = new PKTResRoomEnter()
            {
                Result = (short)errorCode
            };

            var bodyData = MessagePackSerializer.Serialize(resRoomEnter);
            var sendData = PacketToBytes.Make(PACKETID.RES_ROOM_ENTER, bodyData);

            ServerNetwork.SendData(sessionID, sendData);
        }

        ServerPacketData MakeInternalRoomEnterPacket(int roomNumber, string userID, string sessionID, int sessionIndex)
        {
            var packet = new CommonServerLib.PKTInternalReqRoomEnter()
            {
                RoomNumber = roomNumber,
                UserID = userID,
             };

            var packetBodyData = MessagePackSerializer.Serialize(packet);

            var internalPacket = new ServerPacketData();
            internalPacket.Assign(sessionID, sessionIndex, (Int16)PACKETID.REQ_IN_ROOM_ENTER, packetBodyData);
            return internalPacket;
        }


        public void ResponseRoomEnterInternal(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            MainServer.WriteLog("ResponseRoomEnterInternal", LOG_LEVEL.DEBUG);

            try
            {
                var resData = MessagePackSerializer.Deserialize<PKTInternalResRoomEnter>(packetData.BodyData);

                var user = UserMgr.GetUser(sessionIndex);

                if(user == null || user.ID() != resData.UserID)
                {
                    if (resData.Result == ERROR_CODE.NONE)
                    {
                        SendInternalRoomLeavePacket(resData.RoomNumber, resData.UserID);
                    }
                    return;
                }

                if (resData.Result != ERROR_CODE.NONE)
                {                    
                    SessionManager.SetStateLogin(sessionIndex);
                }
                else
                {
                    SessionManager.SetRoomEntered(sessionIndex, resData.RoomNumber);
                }

                ResponseEnterRoomToClient(resData.Result, sessionID);

                MainServer.WriteLog("ResponseRoomEnterInternal - Success", LOG_LEVEL.DEBUG);
            }
            catch (Exception ex)
            {
                MainServer.WriteLog(ex.ToString(), LOG_LEVEL.DEBUG);
            }
        }

        void SendInternalRoomLeavePacket(int roomNumber, string userID)
        {
            // 구현하기
            var packet = new CommonServerLib.PKTInternalNtfRoomLeave()
            {
                RoomNumber = roomNumber,
                UserID = userID,
            };

            var packetBodyData = MessagePackSerializer.Serialize(packet);

            var internalPacket = new ServerPacketData();
            internalPacket.Assign("", -1, (Int16)PACKETID.NTF_IN_ROOM_LEAVE, packetBodyData);

            SendInternalRoomProcessor(false, roomNumber, internalPacket);
        }



        // 테스트 ------------------------------------------------------------
        public void RequestTestEcho(ServerPacketData requestData)
        {
            var session = ServerNetwork.GetSessionByID(requestData.SessionID);

            if(session == null)
            {
                return;
            }

            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(BitConverter.GetBytes((Int32)PACKETID.RES_TEST_ECHO));
            dataSource.AddRange(BitConverter.GetBytes((Int16)0));
            dataSource.AddRange(BitConverter.GetBytes((Int16)0));
            dataSource.AddRange(BitConverter.GetBytes((Int32)requestData.BodyData.Length));
            dataSource.AddRange(requestData.BodyData);

            session.Send(dataSource.ToArray(), 0, dataSource.Count);
        }
    }
}
