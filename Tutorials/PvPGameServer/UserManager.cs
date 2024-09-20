using System;
using System.Collections.Generic;
using System.Linq;


namespace PvPGameServer;

public class UserManager
{
    int _maxUserCount;
    UInt64 _userSequenceNumber = 0;

    Dictionary<string, User> _userMap = new Dictionary<string, User>();


    public void Init(int maxUserCount)
    {
        _maxUserCount = maxUserCount;
    }

    public ErrorCode AddUser(string userID, string sessionID)
    {
        if(IsFullUserCount())
        {
            return ErrorCode.LoginFullUserCount;
        }

        if (_userMap.ContainsKey(sessionID))
        {
            return ErrorCode.AddUserDuplication;
        }


        ++_userSequenceNumber;
        
        var user = new User();
        user.Set(_userSequenceNumber, sessionID, userID);
        _userMap.Add(sessionID, user);

        return ErrorCode.None;
    }

    public ErrorCode RemoveUser(string sessionID)
    {
        if(_userMap.Remove(sessionID) == false)
        {
            return ErrorCode.RemoveUserSearchFailureUserId;
        }

        return ErrorCode.None;
    }

    public User GetUser(string sessionID)
    {
        User user = null;
        _userMap.TryGetValue(sessionID, out user);
        return user;
    }

    bool IsFullUserCount()
    {
        return _maxUserCount <= _userMap.Count();
     }
            
}

public class User
{
    UInt64 _sequenceNumber = 0;
    string _sessionID;
   
    public int RoomNumber { get; private set; } = -1;
    string _userID;

            
    public void Set(UInt64 sequence, string sessionID, string userID)
    {
        _sequenceNumber = sequence;
        _sessionID = sessionID;
        _userID = userID;
    }                   
    
    public bool IsConfirm(string netSessionID)
    {
        return _sessionID == netSessionID;
    }

    public string ID()
    {
        return _userID;
    }

    public void EnteredRoom(int roomNumber)
    {
        RoomNumber = roomNumber;
    }

    public void LeaveRoom()
    {
        RoomNumber = -1;
    }

    public bool IsStateLogin() { return _sequenceNumber != 0; }

    public bool IsStateRoom() { return RoomNumber != -1; }
}

