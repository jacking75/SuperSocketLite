using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSBaseLib;

namespace GameServer;

public class UserManager
{
    int MaxUserCount;
   UInt64 UserSequenceNumber = 0;

    Dictionary<int, User> UserMap = new Dictionary<int, User>();

    public void Init(int maxUserCount)
    {
        MaxUserCount = maxUserCount;
    }

    public ERROR_CODE AddUser(string userID, string sessionID, int sessionIndex)
    {
        if(IsFullUserCount())
        {
            return ERROR_CODE.LOGIN_FULL_USER_COUNT;
        }

        if (UserMap.ContainsKey(sessionIndex))
        {
            return ERROR_CODE.ADD_USER_DUPLICATION;
        }


        ++UserSequenceNumber;
        
        var user = new User();
        user.Set(UserSequenceNumber, sessionID, sessionIndex, userID);
        UserMap.Add(sessionIndex, user);

        return ERROR_CODE.NONE;
    }

    public ERROR_CODE RemoveUser(int sessionIndex)
    {
        if(UserMap.Remove(sessionIndex) == false)
        {
            return ERROR_CODE.REMOVE_USER_SEARCH_FAILURE_USER_ID;
        }

        return ERROR_CODE.NONE;
    }

    public User GetUser(int sessionIndex)
    {
        User user = null;
        UserMap.TryGetValue(sessionIndex, out user);
        return user;
    }

    bool IsFullUserCount()
    {
        return MaxUserCount <= UserMap.Count();
     }
            
}

public class User
{
    UInt64 SequenceNumber = 0;
    string SessionID;
    int SessionIndex = -1;
    public int RoomNumber { get; private set; } = -1;
    string UserID;
            
    public void Set(UInt64 sequence, string sessionID, int sessionIndex, string userID)
    {
        SequenceNumber = sequence;
        SessionID = sessionID;
        SessionIndex = sessionIndex;
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

    public bool IsStateLogin() { return SessionIndex != -1; }

    public bool IsStateRoom() { return RoomNumber != -1; }
}

