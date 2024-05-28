using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class MainServerOption
    {
        public string PvpServerAddress = "";

        public int ChatServerUniqueID { get; set; }

        public string Name { get; set; }

        public int MaxConnectionNumber { get; set; }

        public int Port { get; set; }

        public int MaxRequestLength { get; set; }

        public int ReceiveBufferSize { get; set; }

        public int SendBufferSize { get; set; }

        public int RoomMaxCount { get; set; } = 0;

        public int RoomMaxUserCount { get; set; } = 0;

        public int RoomStartNumber { get; set; } = 0;

        // DBOption
        public string MysqlConnectionStr = "Server=localhost;Port=3306;Database=gamedb;Uid=root;Pwd=0000;";
        public string RedisConnectionStr = "localhost:6380";
        public string MatchRequestListKey { get; set; } = "matchRequest";
        public string MatchCompleteListKey { get; set; } = "matchComplete";


        // TimerOption
        public int CheckAtOnceRoomCount { get; set; } // 한번에 검사할 룸의 개수
        public int CheckFrequencyRoom { get; set; } // 방을 검사하는 빈도수

        public int CheckFrequencyUser { get; set; } // 유저 입장에서 검사받는 빈도수 ex) 250 -> 0.25초에 한번
        public int CheckAtOnceUserRatio { get; set; } // 한번에 검사할 유저수의 비율 ex) 4 -> 한번에 1 / 4 유저씩 검사
        public double MaxPingDelayTime { get; set; } // 핑이 도착해야할 최대 지연 시간


        // ThreadOption
        public int DBThreadCount { get; set; }
    }
}
