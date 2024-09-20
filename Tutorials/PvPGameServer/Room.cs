using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;


namespace PvPGameServer;

public class Room
{
    public const int InvalidRoomNumber = -1;


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

    public bool AddUser(string userID, string netSessionID)
    {
        if(GetUser(userID) != null)
        {
            return false;
        }

        var roomUser = new RoomUser();
        roomUser.Set(userID, netSessionID);
        _userList.Add(roomUser);

        return true;
    }

    public void RemoveUser(string netSessionID)
    {
        var index = _userList.FindIndex(x => x.NetSessionID == netSessionID);
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

    public RoomUser GetUserByNetSessionId(string netSessionID)
    {
        return _userList.Find(x => x.NetSessionID == netSessionID);
    }

    public int CurrentUserCount()
    {
        return _userList.Count();
    }

    public void SendNotifyPacketUserList(string userNetSessionID)
    {
        var packet = new PKTNtfRoomUserList();
        foreach (var user in _userList)
        {
            packet.UserIDList.Add(user.UserID);
        }

        var sendPacket = MemoryPackSerializer.Serialize(packet);
        MemoryPackPacketHeader.Write(sendPacket, PacketId.NtfRoomUserList);
        
        NetSendFunc(userNetSessionID, sendPacket);
    }

    public void SendNofifyPacketNewUser(string newUserNetSessionID, string newUserID)
    {
        var packet = new PKTNtfRoomNewUser();
        packet.UserID = newUserID;
        
        var sendPacket = MemoryPackSerializer.Serialize(packet);
        MemoryPackPacketHeader.Write(sendPacket, PacketId.NtfRoomNewUser);
        
        Broadcast(newUserNetSessionID, sendPacket);
    }

    public void SendNotifyPacketLeaveUser(string userID)
    {
        if(CurrentUserCount() == 0)
        {
            return;
        }

        var packet = new PKTNtfRoomLeaveUser();
        packet.UserID = userID;
        
        var sendPacket = MemoryPackSerializer.Serialize(packet);
        MemoryPackPacketHeader.Write(sendPacket, PacketId.NtfRoomLeaveUser);
      
        Broadcast("", sendPacket);
    }

    public void Broadcast(string excludeNetSessionID, byte[] sendPacket)
    {
        foreach(var user in _userList)
        {
            if(user.NetSessionID == excludeNetSessionID)
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
    public string NetSessionID { get; private set; }

    public void Set(string userID, string netSessionID)
    {
        UserID = userID;
        NetSessionID = netSessionID;
    }
}
