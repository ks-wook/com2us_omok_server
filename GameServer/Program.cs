using System.Collections.Specialized;
using System.Text.Json;

namespace GameServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainServerOption option = InitServerOption();

            Console.WriteLine("--------- Server Start ---------");

            // 리스너 생성 및 실행
            MainServer mainServer = new MainServer();
            mainServer.Init(option);

            mainServer.CreateAndStart();

            while(true)
            {
                System.Threading.Thread.Sleep(50);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.KeyChar == 'q')
                    {
                        Console.WriteLine("--------- Server Terminate ---------");
                        mainServer.StopServer();
                        break;
                    }
                }
            }
        }

        // 서버 옵션 객체를 생성, 조립 후 반환
        static MainServerOption InitServerOption()
        {
            string jsonString = File.ReadAllText("appsettings.json");
            JsonDocument jsonDocument = JsonDocument.Parse(jsonString);

            MainServerOption option = new MainServerOption();

            var connectionStrings = jsonDocument.RootElement.GetProperty("ConnectionStrings");
            var mainServerOption = jsonDocument.RootElement.GetProperty("MainServerOption");
            var matchOption = jsonDocument.RootElement.GetProperty("MatchOption");
            var timerOption = jsonDocument.RootElement.GetProperty("TimerOption");
            var threadOption = jsonDocument.RootElement.GetProperty("ThreadOption");

            // connection strings
            option.MysqlConnectionStr = connectionStrings.GetProperty("MysqlConnectionStr").GetString();
            option.RedisConnectionStr = connectionStrings.GetProperty("RedisConnectionStr").GetString();

            // MainServerOption
            option.Name = mainServerOption.GetProperty("Name").GetString();
            option.Port = mainServerOption.GetProperty("Port").GetInt32();
            option.SendBufferSize = mainServerOption.GetProperty("SendBufferSize").GetInt32();
            option.ReceiveBufferSize = mainServerOption.GetProperty("ReceiveBufferSize").GetInt32();
            option.MaxConnectionNumber = mainServerOption.GetProperty("MaxConnectionNumber").GetInt32();
            option.RoomMaxCount = mainServerOption.GetProperty("RoomMaxCount").GetInt32();
            option.RoomStartNumber = mainServerOption.GetProperty("RoomStartNumber").GetInt32();
            option.MaxRequestLength = mainServerOption.GetProperty("MaxRequestLength").GetInt32();
            option.RoomMaxUserCount = mainServerOption.GetProperty("RoomMaxUserCount").GetInt32();

            // MatchOption
            option.MatchRequestListKey = matchOption.GetProperty("MatchRequestListKey").GetString();
            option.MatchCompleteListKey = matchOption.GetProperty("MatchCompleteListKey").GetString();

            // TimerOption
            option.CheckAtOnceRoomCount = timerOption.GetProperty("CheckAtOnceRoomCount").GetInt32();
            option.CheckFrequencyRoom = timerOption.GetProperty("CheckFrequencyRoom").GetInt32();
            option.CheckFrequencyUser = timerOption.GetProperty("CheckFrequencyUser").GetInt32();
            option.CheckAtOnceUserRatio = timerOption.GetProperty("CheckAtOnceUserRatio").GetInt32();
            option.MaxPingDelayTime = timerOption.GetProperty("MaxPingDelayTime").GetDouble();

            // ThreadOption
            option.DBThreadCount = threadOption.GetProperty("DBThreadCount").GetInt32();

            return option;
        }
    }
}
