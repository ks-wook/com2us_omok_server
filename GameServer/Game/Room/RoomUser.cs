using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class RoomUser
    {
        public string UserId { get; private set; } = string.Empty;
        public string RoomSessionID { get; private set; } = string.Empty;

        public void Set(string userID, string roomSessionId)
        {
            UserId = userID;
            RoomSessionID = roomSessionId;
        }
    }
}
