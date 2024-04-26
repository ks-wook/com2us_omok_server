using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class UserManager
    {
        int MaxConnectionNumber;

        List<User> users = new List<User>();

        public void Init(MainServerOption option) 
        {
            MaxConnectionNumber = option.MaxConnectionNumber;
        }

        public bool AddUser(string userId, string SessionId)
        {
            if (CheckMaxConnection() == false)
            {
                return false;
            }

            users.Add(new User(userId, SessionId));
            return true;
        }

        bool CheckMaxConnection()
        {
            if (users.Count < MaxConnectionNumber) { return true; }

            return false;
        }

        public User? GetUserBySessionId(string SessionId)
        {
            return users.Find(u => u.SessionId == SessionId);
        }

        public User? GetUserByUID(string uid)
        {
            return users.Find(u => uid == u.SessionId);
        }


    }
}
