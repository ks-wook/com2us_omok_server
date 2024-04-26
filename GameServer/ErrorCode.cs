using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public enum ErrorCode
    {
        None = 0,

        // Network 20001 ~


        // Room 21001 ~ 
        NullRoom = 21001,
        ExceedMaxRoomUser = 21002,
        AlreadyExsistUser = 21003,


        // User 22001 ~
        NullUser = 22001,
        ExceedMaxUserConnection = 22002,




    }
}
