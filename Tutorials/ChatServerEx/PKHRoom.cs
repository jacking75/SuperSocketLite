using System;
using System.Collections.Generic;
using System.Linq;

using MessagePack;

using CSBaseLib;
using DB;


namespace ChatServer;

public class PKHRoom : PKHandler
{
    List<Room> _roomList = null;
    int _startRoomNumber;
    
    public void Init(List<Room> roomList)
    {
        _roomList = roomList;
        _startRoomNumber = roomList[0].Number;
    }

    public void RegistPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PacketId.ReqRoomLeave, HandleRequestLeave);
        packetHandlerMap.Add((int)PacketId.NtfInRoomLeave, HandleNotifyLeaveInternal);
        packetHandlerMap.Add((int)PacketId.ReqRoomChat, HandleRequestChat);


        packetHandlerMap.Add((int)PacketId.ReqInRoomEnter, RequestEnterInternal);
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

    (Room, RoomUser) GetRoomAndRoomUser(int userNetSessionIndex)
    {
        Room room = null;
        RoomUser user = null;

        var roomNumber = _sessionMgr.GetRoomNumber(userNetSessionIndex);
        
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
        MainServer.s_MainLogger.Debug("방 입장 요청 받음");

        try
        {
             var reqData = MessagePackSerializer.Deserialize<PKTInternalReqRoomEnter>(packetData.BodyData);

            var room = GetRoom(reqData.RoomNumber);

            if(room == null)
            {
                SendInternalRoomEnterPacketToCommon(ErrorCode.RoomEnterInvalidRoomNumber, reqData.RoomNumber, "", sessionID, sessionIndex);
                return;
            }

            if(room.AddUser(reqData.UserID, sessionIndex, sessionID) == false)
            {
                SendInternalRoomEnterPacketToCommon(ErrorCode.RoomEnterFailAddUser, reqData.RoomNumber, "", sessionID, sessionIndex);
                return;
            }


            room.SendNotifyPacketUserList(sessionID);
            room.SendNofifyPacketNewUser(sessionIndex, reqData.UserID);

            SendInternalRoomEnterPacketToCommon(ErrorCode.None, reqData.RoomNumber, reqData.UserID, sessionID, sessionIndex);

            MainServer.s_MainLogger.Debug("RequestEnterInternal - Success");
        }
        catch (Exception ex)
        {
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }

    void SendInternalRoomEnterPacketToCommon(ErrorCode result, int roomNumber, string userID, string sessionID, int sessionIndex)
    {
        var packet = new PKTInternalResRoomEnter()
        {
            Result = result,
            RoomNumber = roomNumber,
            UserID = userID,
        };

        var packetBodyData = MessagePackSerializer.Serialize(packet);

        var internalPacket = new ServerPacketData();
        internalPacket.Assign(sessionID, sessionIndex, (Int16)PacketId.ResInRoomEnter, packetBodyData);

        SendInternalCommonPacket(internalPacket);
    }


    public void HandleRequestLeave(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        var sessionIndex = packetData.SessionIndex;
        MainServer.s_MainLogger.Debug("로그인 요청 받음");

        try
        {
            if(LeaveRoomUser(sessionIndex) == false)
            {
                return;
            }
            
            _sessionMgr.SetStateLogin(sessionIndex);

            SendResponseLeaveRoomToClient(sessionID);

            MainServer.s_MainLogger.Debug("Room RequestLeave - Success");
        }
        catch (Exception ex)
        {
            MainServer.s_MainLogger.Error(ex.ToString());
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
        var sessionIndex = packetData.SessionIndex;
        LeaveRoomUser(sessionIndex);
    }



    public void HandleRequestChat(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        var sessionIndex = packetData.SessionIndex;
        MainServer.s_MainLogger.Debug("Room RequestChat");

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
            var sendData = PacketToBytes.Make(PacketId.NtfRoomChat, Body);

            roomObject.Item2.Broadcast(-1, sendData);

            MainServer.s_MainLogger.Debug("Room RequestChat - Success");
        }
        catch (Exception ex)
        {
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }
   

    

}
