using GameServer.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class Room
    {
        public int RoomNumber { get; set; }
        public int RoomMaxUserCount { get; set; }
        List<RoomUser> _roomUsers = new List<RoomUser>();

        public void Init(int roomNumber, int roomMaxUserCount)
        {
            RoomNumber = roomNumber;
            RoomMaxUserCount = roomMaxUserCount;
        }


        public ErrorCode AddRoomUser(string uid,  string roomSessionId)
        {
            if(_roomUsers.Count >= RoomMaxUserCount) // 방 정원 초과
            {
                return ErrorCode.ExceedMaxRoomUser;
            }

            if(GetRoomUserByUid(uid) != null)
            {
                return ErrorCode.AlreadyExsistUser;
            }

            var roomUser = new RoomUser();
            roomUser.Set(uid, roomSessionId);
            _roomUsers.Add(roomUser);
            return ErrorCode.None;
        }


        public RoomUser? GetRoomUserByUid(string uid) 
        {
            return _roomUsers.Find(ru => ru.UID == uid);
        }

        public bool RemoveUserByUid(RoomUser ru)
        {
            return _roomUsers.Remove(ru);
        }

        public List<RoomUser> GetRoomUserList()
        {
            return _roomUsers;
        }

    }

    
}
