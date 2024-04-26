using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
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
