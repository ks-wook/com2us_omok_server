namespace GameServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--------- Server Start ---------");

            // 리스너 생성 및 실행
            MainServer mainServer = new MainServer();
            mainServer.Init();

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
