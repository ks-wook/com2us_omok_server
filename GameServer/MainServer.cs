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
using MemoryPack;

namespace GameServer
{
    // AppServer를 상속받는 Listener 생성
    public class MainServer : AppServer<ClientSession, MemoryPackBinaryRequestInfo>
    {
        SuperSocket.SocketBase.Config.IServerConfig _serverConfig;

        PacketProcessor _mainPacketProcessor = new PacketProcessor();
        MysqlProcessor _mysqlProcessor = new MysqlProcessor();
        RedisProcessor _redisProcessor = new RedisProcessor();

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
            _mysqlProcessor.Destory();
            _redisProcessor.Destory();
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

            // 메인 패킷 프로세서
            _mainPacketProcessor.CreateAndStart(_roomManager, _userManager, _mysqlProcessor, _redisProcessor); // 프로세서 초기화

            // mysql 프로세서
            _mysqlProcessor.CreateAndStart(_roomManager, _userManager, _mainServerOption.MysqlConnectionStr, _mainPacketProcessor); // 프로세서 초기화

            // redis 프로세서
            _redisProcessor.CreateAndStart(_roomManager, _userManager, _mainServerOption.RedisConnectionStr, _mainPacketProcessor); // 프로세서 초기화
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

            // 강제 종료시 유저가 방에 있었다면 내보낸다.
            User? user = _userManager.GetUserBySessionId(session.SessionID);
            if (user != null)
            {
                // 게임 중이거나 방에 있던 상태였다면
                if(user.State == UserState.InGame || user.State == UserState.InRoom) 
                {
                    Room? room = _roomManager.FindRoomByRoomNumber(user.RoomNumber);
                    if (room != null)
                    {
                        room.RemoveUserBySessionId(session.SessionID);

                        // 방에 남아있는 유저에게도 접속 종료 패킷 전송
                        PKTResRoomLeave leaveRoomReq = new PKTResRoomLeave();
                        leaveRoomReq.UserId = user.Id;
                        room.NotifyRoomUsers(SendData, leaveRoomReq, PACKETID.PKTResRoomLeave);
                    }
                }


                // 유저 매니저에서 삭제
                _userManager.RemoveUserBySessionId(session.SessionID);
            }

        }

        void OnPacketReceived(ClientSession clientSession, MemoryPackBinaryRequestInfo requestInfo)
        {
            Console.WriteLine($"세션 번호 {clientSession.SessionID} 받은 데이터 크기: {requestInfo.Body.Length}, ThreadId: {Thread.CurrentThread.ManagedThreadId}");
            
            requestInfo.SessionID = clientSession.SessionID;
            _mainPacketProcessor.Insert(requestInfo);
        }

    }
}
