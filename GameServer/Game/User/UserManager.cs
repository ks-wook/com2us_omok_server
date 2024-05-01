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

        public ErrorCode AddUser(string userId, string SessionId)
        {
            if (CheckMaxConnection() == false)
            {
                return ErrorCode.ExceedMaxUserConnection;
            }

            if (CheckExistUser(userId) == false)
            {
                return ErrorCode.AlreadyExsistUser;
            }

            users.Add(new User(userId, SessionId));
            return ErrorCode.None;
        }

        public ErrorCode RemoveUserBySessionId(string sessionId)
        {
            User? user = users.Find(u => u.SessionId == sessionId);
            if(user == null)
            {
                return ErrorCode.NullUser;
            }

            users.Remove(user);

            return ErrorCode.None;
        }

        bool CheckMaxConnection()
        {
            if (users.Count < MaxConnectionNumber) { return true; }

            return false;
        }

        bool CheckExistUser(string userId)
        {
            if(users.Find(u => u.Id == userId) != null) // 이미 접속한 유저
            { 
                return false; 
            }

            return true;
        }

        public User? GetUserBySessionId(string SessionId)
        {
            return users.Find(u => u.SessionId == SessionId);
        }

        public User? GetUserByUID(string uid)
        {
            return users.Find(u => uid == u.Id);
        }


    }
}
