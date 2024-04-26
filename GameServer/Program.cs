namespace GameServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // TODO 명령어로부터 서버 설정값 받아오기





            // TEST 서버 설정값 직접 생성 후 초기화
            MainServerOption option = new MainServerOption();
            {
                option.Name = "OmokGameServer";
                option.Port = 8282;
                option.SendBufferSize = 8000;
                option.ReceiveBufferSize = 8000;
                option.MaxConnectionNumber = 10;
                option.RoomMaxCount = 10;
                option.RoomStartNumber = 1;
                option.RoomMaxUserCount = 2; // 2인 대전 게임
            }
            





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
    }
}
