using CSBaseLib;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;


namespace ChatServer;

public class Room
{
    public int Index { get; private set; }
    public int Number { get; private set; }

    int _maxUserCount = 0;

    List<RoomUser> _userList = new List<RoomUser>();

    public static Func<string, byte[], bool> NetSendFunc;


    public void Init(int index, int number, int maxUserCount)
    {
        Index = index;
        Number = number;
        _maxUserCount = maxUserCount;
    }

    public bool AddUser(string userID, int netSessionIndex, string netSessionID)
    {
        if(GetUser(userID) != null)
        {
            return false;
        }

        var roomUser = new RoomUser();
        roomUser.Set(userID, netSessionIndex, netSessionID);
        _userList.Add(roomUser);

        return true;
    }

    public void RemoveUser(int netSessionIndex)
    {
        var index = _userList.FindIndex(x => x.NetSessionIndex == netSessionIndex);
        _userList.RemoveAt(index);
    }

    public bool RemoveUser(RoomUser user)
    {
        return _userList.Remove(user);
    }

    public RoomUser GetUser(string userID)
    {
        return _userList.Find(x => x.UserID == userID);
    }

    public RoomUser GetUser(int netSessionIndex)
    {
        return _userList.Find(x => x.NetSessionIndex == netSessionIndex);
    }

    public int CurrentUserCount()
    {
        return _userList.Count();
    }

    public void SendNotifyPacketUserList(string userNetSessionID)
    {
        var packet = new CSBaseLib.PKTNtfRoomUserList();
        foreach (var user in _userList)
        {
            packet.UserIDList.Add(user.UserID);
        }

        var bodyData = MessagePackSerializer.Serialize(packet);
        var sendPacket = PacketToBytes.Make(PacketId.NtfRoomUserList, bodyData);

        NetSendFunc(userNetSessionID, sendPacket);
    }

    public void SendNofifyPacketNewUser(int newUserNetSessionIndex, string newUserID)
    {
        var packet = new PKTNtfRoomNewUser();
        packet.UserID = newUserID;
        
        var bodyData = MessagePackSerializer.Serialize(packet);
        var sendPacket = PacketToBytes.Make(PacketId.NtfRoomNewUser, bodyData);

        Broadcast(newUserNetSessionIndex, sendPacket);
    }

    public void SendNotifyPacketLeaveUser(string userID)
    {
        if(CurrentUserCount() == 0)
        {
            return;
        }

        var packet = new PKTNtfRoomLeaveUser();
        packet.UserID = userID;
        
        var bodyData = MessagePackSerializer.Serialize(packet);
        var sendPacket = PacketToBytes.Make(PacketId.NtfRoomLeaveUser, bodyData);

        Broadcast(-1, sendPacket);
    }

    public void Broadcast(int excludeNetSessionIndex, byte[] sendPacket)
    {
        foreach(var user in _userList)
        {
            if(user.NetSessionIndex == excludeNetSessionIndex)
            {
                continue;
            }

            NetSendFunc(user.NetSessionID, sendPacket);
        }
    }
}


public class RoomUser
{
    public string UserID { get; private set; }
    public int NetSessionIndex { get; private set; }
    public string NetSessionID { get; private set; }


    public void Set(string userID, int netSessionIndex, string netSessionID)
    {
        UserID = userID;
        NetSessionIndex = netSessionIndex;
        NetSessionID = netSessionID;
    }
}
