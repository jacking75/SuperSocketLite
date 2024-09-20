using CSBaseLib;
using System;
using System.Threading;


namespace ChatServer;

enum SessionStatus
{
    None = 0,
    Logining = 1,
    Login = 2,
    RoomEntering = 3,
    Room = 4,
}

// 연결된 세션 클래스
class ConnectSession
{
    public bool IsEnable = true;
    Int64 _currentState = (Int64)SessionStatus.None;
    string _userID;
    Int64 _roomNumber = PacketDef.InvalidRoomNumber;


    public void Clear()
    {
        IsEnable = true;
        _currentState = (Int64)SessionStatus.None;
        _roomNumber = PacketDef.InvalidRoomNumber;
    }

    public bool IsStateNone()
    {
        return (IsEnable && _currentState == (Int64)SessionStatus.None);
    }

    public bool IsStateLogin()
    {
        return (IsEnable && _currentState == (Int64)SessionStatus.Login);
    }

    public bool IsStateRoom()
    {
        return (IsEnable && GetState() == SessionStatus.Room);
    }

    public void SetDisable()
    {
        IsEnable = false;
    }

    public void SetStateNone()
    {
        if (IsEnable)
        {
            _currentState = (int)SessionStatus.None;
        }
    }

    public void SetStateLogin()
    {
        if (IsEnable)
        {
            _currentState = (Int64)SessionStatus.Login;
            Interlocked.Exchange(ref _roomNumber, PacketDef.InvalidRoomNumber);
        }
     }

    public void SetStatePreLogin()
    {
        if (IsEnable)
        {
            _currentState = (Int64)SessionStatus.Logining;
        }           
    }

    public void SetStateLogin(string userID)
    {
        if (IsEnable == false)
        {
            return;
        }

        _currentState = (Int64)SessionStatus.Login;
        _userID = userID;
    }

    public int GetRoomNumber()
    {
        return (int)Interlocked.Read(ref _roomNumber);
    }

    SessionStatus GetState()
    {
        return (SessionStatus)Interlocked.Read(ref _currentState);
    }

    // 방에 완전 입장하기 전 상태로 변경  
    public bool SetPreRoomEnter(int roomNumber)
    {
        if (IsEnable == false)
        {
            return false;
        }

        var oldValue = Interlocked.CompareExchange(ref _currentState, (Int64)SessionStatus.RoomEntering, (Int64)SessionStatus.Login);
        if (oldValue != (Int64)SessionStatus.Login)
        {
            return false;
        }

        Interlocked.Exchange(ref _roomNumber, roomNumber);
        return true;
    }

    public bool SetRoomEntered(Int64 roomNumber)
    {
        if (IsEnable == false)
        {
            return false;
        }

        var oldValue = Interlocked.CompareExchange(ref _currentState, (Int64)SessionStatus.Room, (Int64)SessionStatus.RoomEntering);
        if (oldValue != (Int64)SessionStatus.RoomEntering)
        {
            return false;
        }

        Interlocked.Exchange(ref _roomNumber, roomNumber);
        return true;
    }
}
