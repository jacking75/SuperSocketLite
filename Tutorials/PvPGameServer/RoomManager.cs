using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPGameServer;

class RoomManager
{
    List<Room> _roomsList = new List<Room>();


    public void CreateRooms(ServerOption serverOpt)
    {
        var maxRoomCount = serverOpt.RoomMaxCount;
        var startNumber = serverOpt.RoomStartNumber;
        var maxUserCount = serverOpt.RoomMaxUserCount;

        for(int i = 0; i < maxRoomCount; ++i)
        {
            var roomNumber = (startNumber + i);
            var room = new Room();
            room.Init(i, roomNumber, maxUserCount);

            _roomsList.Add(room);
        }                                   
    }


    public List<Room> GetRoomsList() 
    { 
        return _roomsList; 
    }
}
