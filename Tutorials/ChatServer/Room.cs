using CSBaseLib;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    // 방에 있는 유저들의 정보 보낸다(방에 막 들어온 사람에게 보낸다)
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

    // 방에 새로 들어오는 유저의 정보를 보낸다(이미 방에 있는 사람들에게 보낸다)
    public void SendNofifyPacketNewUser(string newUserNetSessionID, string newUserID)
    {
        var packet = new PKTNtfRoomNewUser();
        packet.UserID = newUserID;
        
        var bodyData = MessagePackSerializer.Serialize(packet);
        var sendPacket = PacketToBytes.Make(PacketId.NtfRoomNewUser, bodyData);

        Broadcast(newUserNetSessionID, sendPacket);
    }

    // 방에서 나가는 유저의 정보를 보낸다(아직 방에 있는 사람들에게 보낸다)
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

        Broadcast("", sendPacket);
    }

    // 방에 있는 모든 유저들에게 패킷을 보낸다
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
