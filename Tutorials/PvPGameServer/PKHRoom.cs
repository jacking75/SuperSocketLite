using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;


namespace PvPGameServer;

public class PKHRoom : PKHandler
{
    List<Room> _roomList = null;
    int _startRoomNumber;
    

    public void SetRooomList(List<Room> roomList)
    {
        _roomList = roomList;
        _startRoomNumber = roomList[0].Number;
    }

    public void RegistPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerDict)
    {
        packetHandlerDict.Add((int)PacketId.ReqRoomEnter, HandleRequestRoomEnter);
        packetHandlerDict.Add((int)PacketId.ReqRoomLeave, HandleRequestLeave);
        packetHandlerDict.Add((int)PacketId.NtfInRoomLeave, HandleNotifyLeaveInternal);
        packetHandlerDict.Add((int)PacketId.ReqRoomChat, HandleRequestChat);
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



    public void HandleRequestRoomEnter(MemoryPackBinaryRequestInfo packetData)
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


            var reqData = MemoryPackSerializer.Deserialize<PKTReqRoomEnter>(packetData.Data);
            
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

        var sendPacket = MemoryPackSerializer.Serialize(resRoomEnter);
        MemoryPackPacketHeader.Write(sendPacket, PacketId.ResRoomEnter);
        
        NetSendFunc(sessionID, sendPacket);
    }

    public void HandleRequestLeave(MemoryPackBinaryRequestInfo packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.s_MainLogger.Debug("방나가기 요청 받음");

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

        var sendPacket = MemoryPackSerializer.Serialize(resRoomLeave);
        MemoryPackPacketHeader.Write(sendPacket, PacketId.ResRoomLeave);
   
        NetSendFunc(sessionID, sendPacket);
    }

    public void HandleNotifyLeaveInternal(MemoryPackBinaryRequestInfo packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.s_MainLogger.Debug($"NotifyLeaveInternal. SessionID: {sessionID}");

        var reqData = MemoryPackSerializer.Deserialize<PKTInternalNtfRoomLeave>(packetData.Data);            
        LeaveRoomUser(sessionID, reqData.RoomNumber);
    }
            
    public void HandleRequestChat(MemoryPackBinaryRequestInfo packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.s_MainLogger.Debug("Room RequestChat");

        try
        {
            var roomObject = CheckRoomAndRoomUser(sessionID);

            if(roomObject.Item1 == false)
            {
                return;
            }


            var reqData = MemoryPackSerializer.Deserialize<PKTReqRoomChat>(packetData.Data);

            var notifyPacket = new PKTNtfRoomChat()
            {
                UserID = roomObject.Item3.UserID,
                ChatMessage = reqData.ChatMessage
            };

            var sendPacket = MemoryPackSerializer.Serialize(notifyPacket);
            MemoryPackPacketHeader.Write(sendPacket, PacketId.NtfRoomChat);
            
            roomObject.Item2.Broadcast("", sendPacket);

            MainServer.s_MainLogger.Debug("Room RequestChat - Success");
        }
        catch (Exception ex)
        {
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }
   

    

}
