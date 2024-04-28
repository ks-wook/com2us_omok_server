using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class RoomManager
    {
        List<Room> rooms = new List<Room>();

        public void Init(MainServerOption option)
        {
            // 지정된 개수만큼 룸 생성후 풀링
            var roomMaxCount = option.RoomMaxCount;
            var roomStartNumber = option.RoomStartNumber;
            var roomMaxUserCount = option.RoomMaxUserCount;

            for(int i = 0; i <= roomMaxCount; i++)
            {
                var room = new Room();
                room.Init(roomStartNumber + i, roomMaxUserCount);
                rooms.Add(room);
            }
        }

        public List<Room> GetRoomList() {  return rooms; }


        public Room? FindRoomByRoomNumber(int roomNumber)
        {
            return rooms.Find(r => r.RoomNumber == roomNumber);
        }

        
    }
}
