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
        // [Option("uniqueID", Required = true, HelpText = "Server UniqueID")]
        public int ChatServerUniqueID { get; set; }

        // [Option("name", Required = true, HelpText = "Server Name")]
        public string Name { get; set; }

        // [Option("maxConnectionNumber", Required = true, HelpText = "MaxConnectionNumber")]
        public int MaxConnectionNumber { get; set; }

        // [Option("port", Required = true, HelpText = "Port")]
        public int Port { get; set; }

        // [Option("maxRequestLength", Required = true, HelpText = "maxRequestLength")]
        public int MaxRequestLength { get; set; }

        // [Option("receiveBufferSize", Required = true, HelpText = "receiveBufferSize")]
        public int ReceiveBufferSize { get; set; }

        // [Option("sendBufferSize", Required = true, HelpText = "sendBufferSize")]
        public int SendBufferSize { get; set; }

        // [Option("roomMaxCount", Required = true, HelpText = "Max Romm Count")]
        public int RoomMaxCount { get; set; } = 0;

        // [Option("roomMaxUserCount", Required = true, HelpText = "RoomMaxUserCount")]
        public int RoomMaxUserCount { get; set; } = 0;

        // [Option("roomStartNumber", Required = true, HelpText = "RoomStartNumber")]
        public int RoomStartNumber { get; set; } = 0;


        public string MysqlConnectionStr = "Server=localhost;Port=3306;Database=gamedb;Uid=root;Pwd=0000;";
        public string RedisConnectionStr = "localhost:6380";



        // Timer Option
        public int CheckAtOnceRoomCount { get; set; } = 5; // 한번에 검사할 룸의 개수
        public int CheckFrequencyRoom { get; set; } = 5000; // 방을 검사하는 빈도수

        public int CheckFrequencyUser { get; set; } = 250; // 유저 입장에서 검사받는 빈도수 ex) 250 -> 0.25초에 한번
        public int CheckAtOnceUserRatio { get; set; } = 4; // 한번에 검사할 유저수의 비율 ex) 4 -> 한번에 1 / 4 유저씩 검사
        public double MaxPingDelayTime { get; set; } = 0.25; // 핑이 도착해야할 최대 지연 시간
    }
}
