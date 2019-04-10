using CSBaseLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    enum SessionStatus
    {
        NONE = 0,
        LOGIN_ING = 1,
        LOGIN = 2,
        ROOM_ENTERING = 3,
        ROOM = 4,
    }

    class ConnectSession
    {
        public bool IsEnable = true;
        Int64 CurrentState = (Int64)SessionStatus.NONE;
        string UserID;
        Int64 RoomNumber = PacketDef.INVALID_ROOM_NUMBER;

        public void Clear()
        {
            IsEnable = true;
            CurrentState = (Int64)SessionStatus.NONE;
            RoomNumber = PacketDef.INVALID_ROOM_NUMBER;
        }

        public bool IsStateNone()
        {
            return (IsEnable && CurrentState == (Int64)SessionStatus.NONE);
        }

        public bool IsStateLogin()
        {
            return (IsEnable && CurrentState == (Int64)SessionStatus.LOGIN);
        }

        public bool IsStateRoom()
        {
            return (IsEnable && GetState() == SessionStatus.ROOM);
        }

        public void SetDisable()
        {
            IsEnable = false;
        }

        public void SetStateNone()
        {
            if (IsEnable)
            {
                CurrentState = (int)SessionStatus.NONE;
            }
        }

        public void SetStateLogin()
        {
            if (IsEnable)
            {
                CurrentState = (Int64)SessionStatus.LOGIN;
                Interlocked.Exchange(ref RoomNumber, PacketDef.INVALID_ROOM_NUMBER);
            }
         }

        public void SetStatePreLogin()
        {
            if (IsEnable)
            {
                CurrentState = (Int64)SessionStatus.LOGIN_ING;
            }           
        }

        public void SetStateLogin(string userID)
        {
            if (IsEnable == false)
            {
                return;
            }

            CurrentState = (Int64)SessionStatus.LOGIN;
            UserID = userID;
        }

        public int GetRoomNumber()
        {
            return (int)Interlocked.Read(ref RoomNumber);
        }

        SessionStatus GetState()
        {
            return (SessionStatus)Interlocked.Read(ref CurrentState);
        }

        public bool SetPreRoomEnter(int roomNumber)
        {
            if (IsEnable == false)
            {
                return false;
            }

            var oldValue = Interlocked.CompareExchange(ref CurrentState, (Int64)SessionStatus.ROOM_ENTERING, (Int64)SessionStatus.LOGIN);
            if (oldValue != (Int64)SessionStatus.LOGIN)
            {
                return false;
            }

            Interlocked.Exchange(ref RoomNumber, roomNumber);
            return true;
        }

        public bool SetRoomEntered(Int64 roomNumber)
        {
            if (IsEnable == false)
            {
                return false;
            }

            var oldValue = Interlocked.CompareExchange(ref CurrentState, (Int64)SessionStatus.ROOM, (Int64)SessionStatus.ROOM_ENTERING);
            if (oldValue != (Int64)SessionStatus.ROOM_ENTERING)
            {
                return false;
            }

            Interlocked.Exchange(ref RoomNumber, roomNumber);
            return true;
        }
    }
}
