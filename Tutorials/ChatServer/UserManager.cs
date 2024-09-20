using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSBaseLib;

namespace ChatServer;


/// <summary>
/// 유저 관리자 클래스
/// </summary>
public class UserManager
{
    int _maxUserCount;
    UInt64 _userSequenceNumber = 0;

    Dictionary<string, User> _userMap = new ();


    /// <summary>
    /// 초기화
    /// </summary>
    /// <param name="maxUserCount">최대 사용자 수</param>
    public void Init(int maxUserCount)
    {
        _maxUserCount = maxUserCount;
    }

    /// <summary>
    /// 유저 추가
    /// </summary>
    /// <param name="userID">사용자 ID</param>
    /// <param name="sessionID">세션 ID</param>
    /// <returns>오류 코드</returns>
    public ErrorCode AddUser(string userID, string sessionID)
    {
        if (IsFullUserCount())
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

    /// <summary>
    /// 유저 제거
    /// </summary>
    /// <param name="sessionID">세션 ID</param>
    /// <returns>오류 코드</returns>
    public ErrorCode RemoveUser(string sessionID)
    {
        if (_userMap.Remove(sessionID) == false)
        {
            return ErrorCode.RemoveUserSearchFailureUserId;
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// 세션 ID에 해당하는 유저를 가져온다
    /// </summary>
    /// <param name="sessionID">세션 ID</param>
    /// <returns>사용자 객체</returns>
    public User GetUser(string sessionID)
    {
        User user = null;
        _userMap.TryGetValue(sessionID, out user);
        return user;
    }

    private bool IsFullUserCount()
    {
        return _maxUserCount <= _userMap.Count();
    }
}

/// <summary>
/// 유저 클래스
/// </summary>
public class User
{
    UInt64 _sequenceNumber = 0;
    string _sessionID;


    /// <summary>
    /// 유저가 속한 방 번호
    /// </summary>
    public int RoomNumber { get; private set; } = -1;

    string _userID;


    /// <summary>
    /// 유저 정보를 설정합니다.
    /// </summary>
    /// <param name="sequence">일련번호</param>
    /// <param name="sessionID">세션 ID</param>
    /// <param name="userID">사용자 ID</param>
    public void Set(UInt64 sequence, string sessionID, string userID)
    {
        _sequenceNumber = sequence;
        _sessionID = sessionID;
        _userID = userID;
    }

    /// <summary>
    /// 세션 ID가 일치하는지 확인합니다.
    /// </summary>
    /// <param name="netSessionID">네트워크 세션 ID</param>
    /// <returns>확인 결과</returns>
    public bool IsConfirm(string netSessionID)
    {
        return _sessionID == netSessionID;
    }

    /// <summary>
    /// 유저 ID를 반환합니다.
    /// </summary>
    /// <returns>사용자 ID</returns>
    public string ID()
    {
        return _userID;
    }

    /// <summary>
    /// 방에 입장합니다.
    /// </summary>
    /// <param name="roomNumber">방 번호</param>
    public void EnteredRoom(int roomNumber)
    {
        RoomNumber = roomNumber;
    }

    /// <summary>
    /// 방에서 퇴장합니다.
    /// </summary>
    public void LeaveRoom()
    {
        RoomNumber = -1;
    }

    /// <summary>
    /// 로그인 상태인지 확인합니다.
    /// </summary>
    /// <returns>로그인 상태 여부</returns>
    public bool IsStateLogin()
    {
        return _sequenceNumber != 0;
    }

    /// <summary>
    /// 방에 속해 있는지 확인합니다.
    /// </summary>
    /// <returns>방 소속 여부</returns>
    public bool IsStateRoom()
    {
        return RoomNumber != -1;
    }
}

