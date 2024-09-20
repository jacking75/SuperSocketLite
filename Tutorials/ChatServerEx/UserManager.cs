using System;
using System.Collections.Generic;
using System.Linq;

using CSBaseLib;


namespace ChatServer;

class UserManager
{
    int _maxUserCount;
   UInt64 _userSequenceNumber = 0;

    Dictionary<int, User> _userMap = new ();


    public void Init(int maxUserCount)
    {
        _maxUserCount = maxUserCount;
    }

    public ErrorCode AddUser(string userID, string sessionID, int sessionIndex)
    {
        if(IsFullUserCount())
        {
            return ErrorCode.LoginFullUserCount;
        }

        if (_userMap.ContainsKey(sessionIndex))
        {
            return ErrorCode.AddUserDuplication;
        }


        ++_userSequenceNumber;
        
        var user = new User();
        user.Set(_userSequenceNumber, sessionID, sessionIndex, userID);
        _userMap.Add(sessionIndex, user);

        return ErrorCode.None;
    }

    public ErrorCode RemoveUser(int sessionIndex)
    {
        if(_userMap.Remove(sessionIndex) == false)
        {
            return ErrorCode.RemoveUserSearchFailureUserId;
        }

        return ErrorCode.None;
    }

    public User GetUser(int sessionIndex)
    {
        User user = null;
        _userMap.TryGetValue(sessionIndex, out user);
        return user;
    }

    bool IsFullUserCount()
    {
        return _maxUserCount <= _userMap.Count();
     }
            
}

class User
{
    UInt64 _sequenceNumber = 0;
    string _sessionID;
    int _sessionIndex = -1;
    string _userID;
            

    public void Set(UInt64 sequence, string sessionID, int sessionIndex, string userID)
    {
        _sequenceNumber = sequence;
        _sessionID = sessionID;
        _sessionIndex = sessionIndex;
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

    
}

