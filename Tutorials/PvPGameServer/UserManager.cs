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

    public ERROR_CODE AddUser(string userID, string sessionID)
    {
        if(IsFullUserCount())
        {
            return ERROR_CODE.LOGIN_FULL_USER_COUNT;
        }

        if (_userMap.ContainsKey(sessionID))
        {
            return ERROR_CODE.ADD_USER_DUPLICATION;
        }


        ++_userSequenceNumber;
        
        var user = new User();
        user.Set(_userSequenceNumber, sessionID, userID);
        _userMap.Add(sessionID, user);

        return ERROR_CODE.NONE;
    }

    public ERROR_CODE RemoveUser(string sessionID)
    {
        if(_userMap.Remove(sessionID) == false)
        {
            return ERROR_CODE.REMOVE_USER_SEARCH_FAILURE_USER_ID;
        }

        return ERROR_CODE.NONE;
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
    UInt64 SequenceNumber = 0;
    string SessionID;
   
    public int RoomNumber { get; private set; } = -1;
    string UserID;
            
    public void Set(UInt64 sequence, string sessionID, string userID)
    {
        SequenceNumber = sequence;
        SessionID = sessionID;
        UserID = userID;
    }                   
    
    public bool IsConfirm(string netSessionID)
    {
        return SessionID == netSessionID;
    }

    public string ID()
    {
        return UserID;
    }

    public void EnteredRoom(int roomNumber)
    {
        RoomNumber = roomNumber;
    }

    public void LeaveRoom()
    {
        RoomNumber = -1;
    }

    public bool IsStateLogin() { return SequenceNumber != 0; }

    public bool IsStateRoom() { return RoomNumber != -1; }
}

