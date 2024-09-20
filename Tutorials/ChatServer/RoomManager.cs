using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer;

// 방 관리 클래스
class RoomManager
{
    List<Room> _roomList = new List<Room>();


    public void CreateRooms()
    {
        var maxRoomCount = MainServer.s_ServerOption.RoomMaxCount;
        var startNumber = MainServer.s_ServerOption.RoomStartNumber;
        var maxUserCount = MainServer.s_ServerOption.RoomMaxUserCount;

        for(int i = 0; i < maxRoomCount; ++i)
        {
            var roomNumber = (startNumber + i);
            var room = new Room();
            room.Init(i, roomNumber, maxUserCount);

            _roomList.Add(room);
        }                                   
    }


    public List<Room> GetRoomsList() { return _roomList; }
}
