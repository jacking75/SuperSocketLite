using System.Collections.Generic;


namespace GameServer;

class RoomManager
{
    List<Room> _roomList = new List<Room>();


    public void CreateRooms()
    {
        var maxRoomCount = MainServer.ServerOption.RoomMaxCount;
        var startNumber = MainServer.ServerOption.RoomStartNumber;
        var maxUserCount = MainServer.ServerOption.RoomMaxUserCount;

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
