using System.Collections.Generic;


namespace ChatServer;

class RoomManager
{
    List<List<Room>> _roomsList = new List<List<Room>>();


    public void CreateRooms()
    {
        var maxRoomCount = MainServer.s_ServerOption.RoomMaxCountPerThread * MainServer.s_ServerOption.RoomThreadCount;
        var startNumber = MainServer.s_ServerOption.RoomStartNumber;
        var maxUserCount = MainServer.s_ServerOption.RoomMaxUserCount;

        for(int i = 0; i < MainServer.s_ServerOption.RoomThreadCount; ++i)
        {
            _roomsList.Add(new List<Room>());
        }

        int roomsIndex = -1;
        for (int i = 0; i < maxRoomCount; ++i)
        {
            if( i == 0 || (i % MainServer.s_ServerOption.RoomMaxCountPerThread) == 0)
            {
                ++roomsIndex;
            }

            var roomNumber = (startNumber + i);
            var room = new Room();
            room.Init(i, roomNumber, maxUserCount);

            _roomsList[roomsIndex].Add(room);
        }                        
    }

    public List<Room> GetRoomList(int threadIndex)
    {
        return _roomsList[threadIndex];
    }


}
