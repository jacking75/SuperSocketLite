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
        LOGIN = 2,
        ROOM = 4,
    }

    class ConnectSession
    {
        public bool IsEnable = true;
        SessionStatus CurrentState = SessionStatus.NONE;
        string UserID;
        int RoomNumber = PacketDef.INVALID_ROOM_NUMBER;

        public void Clear()
        {
            IsEnable = true;
            CurrentState = SessionStatus.NONE;
            RoomNumber = PacketDef.INVALID_ROOM_NUMBER;
        }

        public bool IsStateNone()
        {
            return (IsEnable && CurrentState == SessionStatus.NONE);
        }

        public bool IsStateLogin()
        {
            return (IsEnable && CurrentState == SessionStatus.LOGIN);
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
                CurrentState = SessionStatus.LOGIN;
                RoomNumber = PacketDef.INVALID_ROOM_NUMBER;
            }
        }
                
        public void SetStateLogin(string userID)
        {
            if (IsEnable == false)
            {
                return;
            }

            CurrentState = SessionStatus.LOGIN;
            UserID = userID;
        }

        public int GetRoomNumber()
        {
            return RoomNumber;
        }

        SessionStatus GetState()
        {
            return CurrentState;
        }
                
        public bool SetRoomEntered(int roomNumber)
        {
            if (IsEnable == false)
            {
                return false;
            }
                        
            CurrentState = SessionStatus.ROOM;
            RoomNumber = roomNumber;
            return true;
        }
    }
}
