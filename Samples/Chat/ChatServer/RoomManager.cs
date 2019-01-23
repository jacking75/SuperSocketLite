using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class RoomManager
    {
        List<List<Room>> RoomsList = new List<List<Room>>();

        public void CreateRooms()
        {
            var maxRoomCount = ChatServerEnvironment.RoomMaxCountPerThread * ChatServerEnvironment.RoomThreadCount;
            var startNumber = ChatServerEnvironment.RoomStartNumber;
            var maxUserCount = ChatServerEnvironment.RoomMaxUserCount;

            for(int i = 0; i < ChatServerEnvironment.RoomThreadCount; ++i)
            {
                RoomsList.Add(new List<Room>());
            }

            int roomsIndex = -1;
            for (int i = 0; i < maxRoomCount; ++i)
            {
                if( i == 0 || (i % ChatServerEnvironment.RoomMaxCountPerThread) == 0)
                {
                    ++roomsIndex;
                }

                var roomNumber = (startNumber + i);
                var room = new Room();
                room.Init(i, roomNumber, maxUserCount);

                RoomsList[roomsIndex].Add(room);
            }                        
        }

        public List<Room> GetRoomList(int threadIndex)
        {
            return RoomsList[threadIndex];
        }


    }
}
