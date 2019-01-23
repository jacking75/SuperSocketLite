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
            packetHandlerMap.Add((int)PACKETID.REQ_ROOM_LEAVE, RequestLeave);
            packetHandlerMap.Add((int)PACKETID.NTF_IN_ROOM_LEAVE, NotifyLeaveInternal);
            packetHandlerMap.Add((int)PACKETID.REQ_ROOM_CHAT, RequestChat);


            packetHandlerMap.Add((int)PACKETID.REQ_IN_ROOM_ENTER, RequestEnterInternal);
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

        public void RequestEnterInternal(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            DevLog.Write("방 입장 요청 받음", LOG_LEVEL.DEBUG);

            try
            {
                 var reqData = MessagePackSerializer.Deserialize<PKTInternalReqRoomEnter>(packetData.BodyData);

                var room = GetRoom(reqData.RoomNumber);

                if(room == null)
                {
                    SendInternalRoomEnterPacketToCommon(ERROR_CODE.ROOM_ENTER_INVALID_ROOM_NUMBER, reqData.RoomNumber, "", sessionID, sessionIndex);
                    return;
                }

                if(room.AddUser(reqData.UserID, sessionIndex, sessionID) == false)
                {
                    SendInternalRoomEnterPacketToCommon(ERROR_CODE.ROOM_ENTER_FAIL_ADD_USER, reqData.RoomNumber, "", sessionID, sessionIndex);
                    return;
                }


                room.NotifyPacketUserList(sessionID);
                room.NofifyPacketNewUser(sessionIndex, reqData.UserID);

                SendInternalRoomEnterPacketToCommon(ERROR_CODE.NONE, reqData.RoomNumber, reqData.UserID, sessionID, sessionIndex);
                    
                DevLog.Write("RequestEnterInternal - Success", LOG_LEVEL.DEBUG);
            }
            catch (Exception ex)
            {
                DevLog.Write(ex.ToString(), LOG_LEVEL.DEBUG);
            }
        }

        void SendInternalRoomEnterPacketToCommon(ERROR_CODE result, int roomNumber, string userID, string sessionID, int sessionIndex)
        {
            var packet = new CommonServerLib.PKTInternalResRoomEnter()
            {
                Result = result,
                RoomNumber = roomNumber,
                UserID = userID,
            };

            var packetBodyData = MessagePackSerializer.Serialize(packet);

            var internalPacket = new ServerPacketData();
            internalPacket.Assign(sessionID, sessionIndex, (Int16)PACKETID.RES_IN_ROOM_ENTER, packetBodyData);

            SendInternalCommonPacket(internalPacket);
        }


        public void RequestLeave(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            DevLog.Write("로그인 요청 받음", LOG_LEVEL.DEBUG);

            try
            {
                if(LeaveRoomUser(sessionIndex) == false)
                {
                    return;
                }
                
                SessionManager.SetStateLogin(sessionIndex);

                ResponseLeaveRoomToClient(sessionID);

                DevLog.Write("Room RequestLeave - Success", LOG_LEVEL.DEBUG);
            }
            catch (Exception ex)
            {
                DevLog.Write(ex.ToString(), LOG_LEVEL.DEBUG);
            }
        }

        bool LeaveRoomUser(int sessionIndex)
        {
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
            var resRoomEnter = new PKTResRoomLeave()
            {
                Result = (short)ERROR_CODE.NONE
            };

            var bodyData = MessagePackSerializer.Serialize(resRoomEnter);
            var sendData = PacketToBytes.Make(PACKETID.RES_ROOM_LEAVE, bodyData);

            ServerNetwork.SendData(sessionID, sendData);
        }

        public void NotifyLeaveInternal(ServerPacketData packetData)
        {
            var sessionIndex = packetData.SessionIndex;
            LeaveRoomUser(sessionIndex);
        }



        public void RequestChat(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            DevLog.Write("Room RequestChat", LOG_LEVEL.DEBUG);

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

                 DevLog.Write("Room RequestChat - Success", LOG_LEVEL.DEBUG);
            }
            catch (Exception ex)
            {
                DevLog.Write(ex.ToString(), LOG_LEVEL.DEBUG);
            }
        }
       

        

    }
}
