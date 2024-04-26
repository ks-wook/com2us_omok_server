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
        List<RoomUser> _roomUsers = new List<RoomUser>();



        public bool AddRoomUser(string uid,  string roomSessionId)
        {
            if(GetRoomUserByUid(uid) != null)
            {
                return false;
            }

            var roomUser = new RoomUser();
            roomUser.Set(uid, roomSessionId);
            _roomUsers.Add(roomUser);
            return true;
        }

        public RoomUser? GetRoomUserByUid(string uid) 
        {
            return _roomUsers.Find(ru => ru.UID == uid);
        }

        public bool RemoveUserByUid(RoomUser ru)
        {
            return _roomUsers.Remove(ru);
        }



    }

    public class RoomUser
    {
        public string UID { get; private set; }
        public string RoomSessionID { get; private set; } // 방에서 구분되는 유저의 아이디

        public void Set(string userID, string roomSessionId)
        {
            UID = userID;
            RoomSessionID = roomSessionId;
        }
    }
}
