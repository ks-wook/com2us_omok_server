using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string SessionId {  get; set; } = string.Empty;

        public User(string id, string sessionId)
        {
            Id = id;
            SessionId = sessionId;
        }


    }
}
