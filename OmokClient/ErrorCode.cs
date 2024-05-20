using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmokClient
{
    public enum ErrorCode
    {
        None = 0,

        // Network 20001 ~
        LoginFail = 20001,
        InvaildToken = 20002,
        AlreadyExsistUser = 20003,
        NullPacket = 20004,

        // Room 21001 ~ 
        NullRoom = 21001,
        ExceedMaxRoomUser = 21002,
        AlreadyExsistRoomUser = 21003,


        // User 22001 ~
        NullUser = 22001,
        ExceedMaxUserConnection = 22002,




    }
}
