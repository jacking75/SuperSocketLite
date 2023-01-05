using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;


namespace PvPGameServer
{
    public class PKHRoom : PKHandler
    {
        List<Room> RoomList = null;
        int StartRoomNumber;
        
        public void SetRooomList(List<Room> roomList)
        {
            RoomList = roomList;
            StartRoomNumber = roomList[0].Number;
        }

        public void RegistPacketHandler(Dictionary<int, Action<EFBinaryRequestInfo>> packetHandlerMap)
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
                
        (bool, Room, RoomUser) CheckRoomAndRoomUser(string userNetSessionID)
        {
            var user = UserMgr.GetUser(userNetSessionID);
            if (user == null)
            {
                return (false, null, null);
            }

            var roomNumber = user.RoomNumber;
            var room = GetRoom(roomNumber);

            if(room == null)
            {
                return (false, null, null);
            }

            var roomUser = room.GetUserByNetSessionId(userNetSessionID);

            if (roomUser == null)
            {
                return (false, room, null);
            }

            return (true, room, roomUser);
        }



        public void RequestRoomEnter(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug("RequestRoomEnter");

            try
            {
                var user = UserMgr.GetUser(sessionID);

                if (user == null || user.IsConfirm(sessionID) == false)
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_USER, sessionID);
                    return;
                }

                if (user.IsStateRoom())
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_STATE, sessionID);
                    return;
                }

                var reqData = MessagePackSerializer.Deserialize<PKTReqRoomEnter>(packetData.Data);
                
                var room = GetRoom(reqData.RoomNumber);

                if (room == null)
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_ROOM_NUMBER, sessionID);
                    return;
                }

                if (room.AddUser(user.ID(), sessionID) == false)
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_FAIL_ADD_USER, sessionID);
                    return;
                }


                user.EnteredRoom(reqData.RoomNumber);

                room.NotifyPacketUserList(sessionID);
                room.NofifyPacketNewUser(sessionID, user.ID());

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

            var sendPacket = MessagePackSerializer.Serialize(resRoomEnter);
            WriteHeaderInfo(PACKETID.RES_ROOM_ENTER, sendPacket);
            
            NetSendFunc(sessionID, sendPacket);
        }

        public void RequestLeave(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug("로그인 요청 받음");

            try
            {
                var user = UserMgr.GetUser(sessionID);
                if(user == null)
                {
                    return;
                }

                if(LeaveRoomUser(sessionID, user.RoomNumber) == false)
                {
                    return;
                }

                user.LeaveRoom();

                ResponseLeaveRoomToClient(sessionID);

                MainServer.MainLogger.Debug("Room RequestLeave - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        bool LeaveRoomUser(string sessionID, int roomNumber)
        {
            MainServer.MainLogger.Debug($"LeaveRoomUser. SessionID:{sessionID}");

            var room = GetRoom(roomNumber);
            if (room == null)
            {
                return false;
            }

            var roomUser = room.GetUserByNetSessionId(sessionID);
            if (roomUser == null)
            {
                return false;
            }
                        
            var userID = roomUser.UserID;
            room.RemoveUser(roomUser);

            room.NotifyPacketLeaveUser(userID);
            return true;
        }

        void ResponseLeaveRoomToClient(string sessionID)
        {
            var resRoomLeave = new PKTResRoomLeave()
            {
                Result = (short)ERROR_CODE.NONE
            };

            var sendPacket = MessagePackSerializer.Serialize(resRoomLeave);
            WriteHeaderInfo(PACKETID.RES_ROOM_LEAVE, sendPacket);
       
            NetSendFunc(sessionID, sendPacket);
        }

        public void NotifyLeaveInternal(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug($"NotifyLeaveInternal. SessionID: {sessionID}");

            var reqData = MessagePackSerializer.Deserialize<PKTInternalNtfRoomLeave>(packetData.Data);            
            LeaveRoomUser(sessionID, reqData.RoomNumber);
        }
                
        public void RequestChat(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug("Room RequestChat");

            try
            {
                var roomObject = CheckRoomAndRoomUser(sessionID);

                if(roomObject.Item1 == false)
                {
                    return;
                }


                var reqData = MessagePackSerializer.Deserialize<PKTReqRoomChat>(packetData.Data);

                var notifyPacket = new PKTNtfRoomChat()
                {
                    UserID = roomObject.Item3.UserID,
                    ChatMessage = reqData.ChatMessage
                };

                var sendPacket = MessagePackSerializer.Serialize(notifyPacket);
                WriteHeaderInfo(PACKETID.NTF_ROOM_CHAT, sendPacket);
                
                roomObject.Item2.Broadcast("", sendPacket);

                MainServer.MainLogger.Debug("Room RequestChat - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }
       

        

    }
}
