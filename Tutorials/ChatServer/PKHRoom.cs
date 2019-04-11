using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;

using CSBaseLib;


namespace ChatServer
{
    public class PKHRoom : PKHandler
    {
        List<Room> RoomList = null;
        int StartRoomNumber;
        
        public void Init(List<Room> roomList)
        {
            RoomList = roomList;
            StartRoomNumber = roomList[0].Number;
        }

        public void RegistPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerMap)
        {
            packetHandlerMap.Add((int)PACKETID.REQ_ROOM_ENTER, RequestRoomEnter);
            packetHandlerMap.Add((int)PACKETID.REQ_ROOM_LEAVE, RequestLeave);
            packetHandlerMap.Add((int)PACKETID.NTF_IN_ROOM_LEAVE, NotifyLeaveInternal);
            packetHandlerMap.Add((int)PACKETID.REQ_ROOM_CHAT, RequestChat);
        }


        Room GetRoom(int roomNumber)
        {
            var index = roomNumber - StartRoomNumber;

            if( index < 0 || index >= RoomList.Count())
            {
                return null;
            }

            return RoomList[index];
        }

        (Room, RoomUser) GetRoomAndRoomUser(int userNetSessionIndex)
        {
            Room room = null;
            RoomUser user = null;

            var roomNumber = SessionManager.GetRoomNumber(userNetSessionIndex);
            
            room = GetRoom(roomNumber);
            if (room == null)
            {
                return (room, user);
            }

            user = room.GetUser(userNetSessionIndex);
            return (room, user);
        }

        (bool, Room, RoomUser) CheckRoomAndRoomUser(int userNetSessionIndex)
        {
            var roomObject = GetRoomAndRoomUser(userNetSessionIndex);

            if(roomObject.Item1 == null || roomObject.Item2 == null)
            {
                return (false, roomObject.Item1, roomObject.Item2);
            }

            return (true, roomObject.Item1, roomObject.Item2);
        }



        public void RequestRoomEnter(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            MainServer.MainLogger.Debug("RequestRoomEnter");

            try
            {
                var user = UserMgr.GetUser(sessionIndex);

                if (user == null || user.IsConfirm(sessionID) == false)
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
                
                var room = GetRoom(reqData.RoomNumber);

                if (room == null)
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_ROOM_NUMBER, sessionID);
                    return;
                }

                if (room.AddUser(user.ID(), sessionIndex, sessionID) == false)
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_FAIL_ADD_USER, sessionID);
                    return;
                }


                SessionManager.SetRoomEntered(sessionIndex, reqData.RoomNumber);

                room.NotifyPacketUserList(sessionID);
                room.NofifyPacketNewUser(sessionIndex, user.ID());

                ResponseEnterRoomToClient(ERROR_CODE.NONE, sessionID);

                MainServer.MainLogger.Debug("RequestEnterInternal - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        void ResponseEnterRoomToClient(ERROR_CODE errorCode, string sessionID)
        {
            var resRoomEnter = new PKTResRoomEnter()
            {
                Result = (short)errorCode
            };

            var bodyData = MessagePackSerializer.Serialize(resRoomEnter);
            var sendData = PacketToBytes.Make(PACKETID.RES_ROOM_ENTER, bodyData);

            ServerNetwork.SendData(sessionID, sendData);
        }

        public void RequestLeave(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            MainServer.MainLogger.Debug("로그인 요청 받음");

            try
            {
                if(LeaveRoomUser(sessionIndex) == false)
                {
                    return;
                }
                
                SessionManager.SetStateLogin(sessionIndex);

                ResponseLeaveRoomToClient(sessionID);

                MainServer.MainLogger.Debug("Room RequestLeave - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        bool LeaveRoomUser(int sessionIndex)
        {
            MainServer.MainLogger.Debug($"LeaveRoomUser. SessionIndex:{sessionIndex}");

            var roomObject = GetRoomAndRoomUser(sessionIndex);
            var room = roomObject.Item1;
            var user = roomObject.Item2;

            if (room == null || user == null)
            {
                return false;
            }

            var userID = user.UserID;
            room.RemoveUser(user);

            room.NotifyPacketLeaveUser(userID);
            return true;
        }

        void ResponseLeaveRoomToClient(string sessionID)
        {
            var resRoomLeave = new PKTResRoomLeave()
            {
                Result = (short)ERROR_CODE.NONE
            };

            var bodyData = MessagePackSerializer.Serialize(resRoomLeave);
            var sendData = PacketToBytes.Make(PACKETID.RES_ROOM_LEAVE, bodyData);

            ServerNetwork.SendData(sessionID, sendData);
        }

        public void NotifyLeaveInternal(ServerPacketData packetData)
        {
            MainServer.MainLogger.Debug($"NotifyLeaveInternal. SessionIndex: {packetData.SessionIndex}");

            var sessionIndex = packetData.SessionIndex;
            LeaveRoomUser(sessionIndex);
        }



        public void RequestChat(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            MainServer.MainLogger.Debug("Room RequestChat");

            try
            {
                var roomObject = CheckRoomAndRoomUser(sessionIndex);

                if(roomObject.Item1 == false)
                {
                    return;
                }


                var reqData = MessagePackSerializer.Deserialize<PKTReqRoomChat>(packetData.BodyData);

                var notifyPacket = new PKTNtfRoomChat()
                {
                    UserID = roomObject.Item3.UserID,
                    ChatMessage = reqData.ChatMessage
                };

                var Body = MessagePackSerializer.Serialize(notifyPacket);
                var sendData = PacketToBytes.Make(PACKETID.NTF_ROOM_CHAT, Body);

                roomObject.Item2.Broadcast(-1, sendData);

                MainServer.MainLogger.Debug("Room RequestChat - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }
       

        

    }
}
