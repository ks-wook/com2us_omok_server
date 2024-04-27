using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Session;
using GameServer.Packet;

namespace GameServer
{
    // AppServer를 상속받는 Listener 생성
    public class MainServer : AppServer<ClientSession, MemoryPackBinaryRequestInfo>
    {
        SuperSocket.SocketBase.Config.IServerConfig _serverConfig;
        PacketProcessor _mainPacketProcessor = new PacketProcessor();
        MainServerOption _mainServerOption;


        // 매니저 생성
        RoomManager _roomManager = new RoomManager();
        UserManager _userManager = new UserManager();



        public MainServer()
            : base(new DefaultReceiveFilterFactory<ReceiveFilter, MemoryPackBinaryRequestInfo>())
        {
            NewSessionConnected += new SessionHandler<ClientSession>(OnConnected);
            SessionClosed += new SessionHandler<ClientSession, CloseReason>(OnClosed);
            NewRequestReceived += new RequestHandler<ClientSession, MemoryPackBinaryRequestInfo>(OnPacketReceived);
        }

        public void Init(MainServerOption option)
        {
            _mainServerOption = option;

            _serverConfig = new SuperSocket.SocketBase.Config.ServerConfig()
            {
                Name = option.Name,
                Ip = "Any", // 모든 주소 연결 허용
                Port = option.Port,
                Mode = SocketMode.Tcp,
                MaxConnectionNumber = option.MaxConnectionNumber, // 최대 동접 수
                MaxRequestLength = option.MaxRequestLength,
                ReceiveBufferSize = option.ReceiveBufferSize, // recv 버퍼 사이즈 2048 할당
                SendBufferSize = option.SendBufferSize, // send 버퍼 사이즈 2048 할당
            };


        }

        public void StopServer()
        {
            Stop();

            _mainPacketProcessor.Destory();
        }

        public void CreateAndStart()
        {
            try
            {
                bool result = Setup(new SuperSocket.SocketBase.Config.RootConfig(), _serverConfig, logFactory: new ConsoleLogFactory());

                if (result == false)
                {
                    Console.WriteLine("ChatServer 초기화 실패");
                    return;
                }
                else if (result == true)
                {
                    Console.WriteLine("ChatServer 초기화 성공");
                }

                CreateComponent();


                Start(); // 서버 Listening 시작
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 서버 생성 실패: {ex.ToString()}");
            }

        }

        // 패킷 프로세서 생성 및 실행
        void CreateComponent()
        {
            // 매니저 초기화
            _userManager.Init(_mainServerOption);
            _roomManager.Init(_mainServerOption);


            PacketHandler.NetSendFunc = SendData;

            _mainPacketProcessor = new PacketProcessor();
            _mainPacketProcessor.CreateAndStart(_roomManager, _userManager); // 프로세서 초기화
        }


        public bool SendData(string sessionID, byte[] sendData)
        {
            var session = GetSessionByID(sessionID);

            try
            {
                if (session == null)
                {
                    return false;
                }

                session.Send(sendData, 0, sendData.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                session.Close();
            }
            return true;
        }


        void OnConnected(ClientSession session)
        {
            Console.WriteLine($"[OnConnected] sesseionID: {session.SessionID}");
        }

        void OnClosed(ClientSession session, CloseReason closeReason)
        {
            Console.WriteLine($"[OnClosed] sesseionID: {session.SessionID}, ${closeReason.ToString()}");
        }

        void OnPacketReceived(ClientSession clientSession, MemoryPackBinaryRequestInfo requestInfo)
        {
            Console.WriteLine($"세션 번호 {clientSession.SessionID} 받은 데이터 크기: {requestInfo.Body.Length}, ThreadId: {Thread.CurrentThread.ManagedThreadId}");

            requestInfo.SessionID = clientSession.SessionID;
            _mainPacketProcessor.Insert(requestInfo);
        }

    }
}
