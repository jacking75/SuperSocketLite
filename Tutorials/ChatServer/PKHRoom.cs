using System;
using System.Collections.Generic;
using System.Linq;

using MessagePack;

using CSBaseLib;


namespace ChatServer;

public class PKHRoom : PKHandler
{
    List<Room> _roomList = null;
    int _startRoomNumber;
    

    public void SetRooomList(List<Room> roomList)
    {
        _roomList = roomList;
        _startRoomNumber = roomList[0].Number;
    }

    public void RegistPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PacketId.ReqRoomEnter, HandleRequestRoomEnter);
        packetHandlerMap.Add((int)PacketId.ReqRoomLeave, HandleRequestLeave);
        packetHandlerMap.Add((int)PacketId.NtfInRoomLeave, HandleNotifyLeaveInternal);
        packetHandlerMap.Add((int)PacketId.ReqRoomChat, HandleRequestChat);
    }


    Room GetRoom(int roomNumber)
    {
        var index = roomNumber - _startRoomNumber;

        if( index < 0 || index >= _roomList.Count())
        {
            return null;
        }

        return _roomList[index];
    }
            
    (bool, Room, RoomUser) CheckRoomAndRoomUser(string userNetSessionID)
    {
        var user = _userMgr.GetUser(userNetSessionID);
        if (user == null)
        {
            return (false, null, null);
        }

        
        var room = GetRoom(user.RoomNumber);
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



    public void HandleRequestRoomEnter(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.s_MainLogger.Debug("RequestRoomEnter");

        try
        {
            var user = _userMgr.GetUser(sessionID);
            if (user == null || user.IsConfirm(sessionID) == false)
            {
                SendResponseEnterRoomToClient(ErrorCode.RoomEnterInvalidUser, sessionID);
                return;
            }

            if (user.IsStateRoom())
            {
                SendResponseEnterRoomToClient(ErrorCode.RoomEnterInvalidState, sessionID);
                return;
            }


            var reqData = MessagePackSerializer.Deserialize<PKTReqRoomEnter>(packetData.BodyData);
            
            var room = GetRoom(reqData.RoomNumber);

            if (room == null)
            {
                SendResponseEnterRoomToClient(ErrorCode.RoomEnterInvalidRoomNumber, sessionID);
                return;
            }

            if (room.AddUser(user.ID(), sessionID) == false)
            {
                SendResponseEnterRoomToClient(ErrorCode.RoomEnterFailAddUser, sessionID);
                return;
            }


            user.EnteredRoom(reqData.RoomNumber);

            room.SendNotifyPacketUserList(sessionID);
            room.SendNofifyPacketNewUser(sessionID, user.ID());

            SendResponseEnterRoomToClient(ErrorCode.None, sessionID);

            MainServer.s_MainLogger.Debug("RequestEnterInternal - Success");
        }
        catch (Exception ex)
        {
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }

    void SendResponseEnterRoomToClient(ErrorCode errorCode, string sessionID)
    {
        var resRoomEnter = new PKTResRoomEnter()
        {
            Result = (short)errorCode
        };

        var bodyData = MessagePackSerializer.Serialize(resRoomEnter);
        var sendData = PacketToBytes.Make(PacketId.ResRoomEnter, bodyData);

        _serverNetwork.SendData(sessionID, sendData);
    }

    public void HandleRequestLeave(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.s_MainLogger.Debug("로그인 요청 받음");

        try
        {
            var user = _userMgr.GetUser(sessionID);
            if(user == null)
            {
                return;
            }

            if(LeaveRoomUser(sessionID, user.RoomNumber) == false)
            {
                return;
            }

            user.LeaveRoom();

            SendResponseLeaveRoomToClient(sessionID);

            MainServer.s_MainLogger.Debug("Room RequestLeave - Success");
        }
        catch (Exception ex)
        {
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }

    bool LeaveRoomUser(string sessionID, int roomNumber)
    {
        MainServer.s_MainLogger.Debug($"LeaveRoomUser. SessionID:{sessionID}");

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

        room.SendNotifyPacketLeaveUser(userID);
        return true;
    }

    void SendResponseLeaveRoomToClient(string sessionID)
    {
        var resRoomLeave = new PKTResRoomLeave()
        {
            Result = (short)ErrorCode.None
        };

        var bodyData = MessagePackSerializer.Serialize(resRoomLeave);
        var sendData = PacketToBytes.Make(PacketId.ResRoomLeave, bodyData);

        _serverNetwork.SendData(sessionID, sendData);
    }


    public void HandleNotifyLeaveInternal(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.s_MainLogger.Debug($"NotifyLeaveInternal. SessionID: {sessionID}");

        var reqData = MessagePackSerializer.Deserialize<PKTInternalNtfRoomLeave>(packetData.BodyData);            
        LeaveRoomUser(sessionID, reqData.RoomNumber);
    }
            
    public void HandleRequestChat(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.s_MainLogger.Debug("Room RequestChat");

        try
        {
            var (isResult, room, roomUser) = CheckRoomAndRoomUser(sessionID);
            
            if(isResult == false)
            {
                return;
            }

            var reqData = MessagePackSerializer.Deserialize<PKTReqRoomChat>(packetData.BodyData);


            var notifyPacket = new PKTNtfRoomChat()
            {
                UserID = roomUser.UserID,
                ChatMessage = reqData.ChatMessage
            };

            var Body = MessagePackSerializer.Serialize(notifyPacket);
            var sendData = PacketToBytes.Make(PacketId.NtfRoomChat, Body);

            room.Broadcast("", sendData);

            MainServer.s_MainLogger.Debug("Room RequestChat - Success");
        }
        catch (Exception ex)
        {
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }
   

    

}
