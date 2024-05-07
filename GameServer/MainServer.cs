using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Packet;
using MemoryPack;
using System.Security.Cryptography.Xml;
using System.Net.Sockets;

namespace GameServer
{
    // AppServer를 상속받는 Listener 생성
    public class MainServer : AppServer<ClientSession, MemoryPackBinaryRequestInfo>
    {
        SuperSocket.SocketBase.Config.IServerConfig _serverConfig;
        SuperSocket.SocketBase.Logging.ILog _mainLogger;

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
                MaxConnectionNumber = option.MaxConnectionNumber,
                MaxRequestLength = option.MaxRequestLength,
                ReceiveBufferSize = option.ReceiveBufferSize, 
                SendBufferSize = option.SendBufferSize, 
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
                bool result = Setup(new SuperSocket.SocketBase.Config.RootConfig(), _serverConfig, logFactory: new NLogLogFactory());

                if (result == false)
                {
                    Console.WriteLine("[Error] GameServer 초기화 실패");
                    return;
                }
                else if (result == true)
                {
                    _mainLogger = base.Logger;
                    _mainLogger.Error("서버 초기화 성공");
                }

                CreateComponent();

                Start(); // 서버 Listening 시작

                _mainLogger.Info("서버 생성 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 서버 생성 실패: {ex.ToString()}");
            }

        }

        // 패킷 프로세서 생성 및 실행
        ErrorCode CreateComponent()
        {
            // 매니저 초기화
            _userManager.Init(_mainServerOption, _mainPacketProcessor.Insert, CloseConnection);
            _roomManager.Init(_mainServerOption, _mainPacketProcessor.Insert);

            BasePacketHandler.NetSendFunc = SendData;

            // 메인 패킷 프로세서
            _mainPacketProcessor.CreateAndStart(_mainLogger, _roomManager, _userManager, _mysqlProcessor.Insert, _redisProcessor.Insert); // 프로세서 초기화

            // mysql 프로세서
            _mysqlProcessor.CreateAndStart(_mainLogger, _mainServerOption.MysqlConnectionStr, _mainPacketProcessor.Insert); // 프로세서 초기화

            // redis 프로세서
            _redisProcessor.CreateAndStart(_mainLogger, _mainServerOption.RedisConnectionStr, _mainPacketProcessor.Insert); // 프로세서 초기화

            _mainLogger.Info("CreateComponent - Success");
            return ErrorCode.None;
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
                _mainLogger.Error($"{ex.ToString()}, {ex.StackTrace}");

                Console.WriteLine(ex.ToString());
                session.Close();
            }
            return true;
        }

        public void CloseConnection(string sessionId)
        {
            var sessions = GetAllSessions();

            foreach (var session in sessions)
            {
                if (sessionId == session.SessionID)
                {
                    session.Close();
                    break;
                }
            }

        }

        public void OnUserClosed(string sessionId)
        {
            // 강제 종료시 유저가 방에 있었다면 내보낸다.
            User? user = _userManager.GetUserBySessionId(sessionId);
            if (user != null)
            {
                // 방에 있던 상태였다면
                if (user.RoomNumber != -1)
                {
                    Room? room = _roomManager.FindRoomByRoomNumber(user.RoomNumber);
                    if (room != null)
                    {
                        room.RemoveUserBySessionId(sessionId);

                        _mainLogger.Info($"[{room.RoomNumber}번 room] Uid {sessionId} 퇴장, 현재 인원: {room.GetRoomUserCount()}");

                        // 방에 남아있는 유저에게도 접속 종료 패킷 전송
                        PKTResRoomLeave leaveRoomReq = new PKTResRoomLeave();
                        leaveRoomReq.UserId = user.Id;
                        room.NotifyRoomUsers(SendData, leaveRoomReq, PACKETID.PKTResRoomLeave);

                        // 게임 중이었다면 게임을 종료 시킨다.
                        if (user.State == UserState.InGame)
                        {
                            // 룸에 사람이 남아있다면 그 사람이 승자가 된다.
                            RoomUser? winRoomUser = room.GetRoomUserIfOneUser();
                            if(winRoomUser != null)
                            {
                                // 종료된 게임을 db에 저장한다.
                                PKTInnerReqSaveGameResult sendDBData = new PKTInnerReqSaveGameResult();
                                foreach (RoomUser ru in room.GetRoomUserList())
                                {
                                    sendDBData.sessionIds.Add(ru.RoomSessionID);
                                }

                                sendDBData.BlackUserId = room.GetOmokGame().BlackUserId;
                                sendDBData.WhiteUserId = room.GetOmokGame().WhiteUserId;
                                sendDBData.WinUserId = winRoomUser.UserId;

                                var sendDbPacket = MemoryPackSerializer.Serialize(sendDBData);
                                MemoryPackPacketHeadInfo.Write(sendDbPacket, InnerPacketId.PKTInnerReqSaveGameResult);
                                _mysqlProcessor.Insert(new MemoryPackBinaryRequestInfo(sendDbPacket));
                            }
                        }
                    } 
                }

                _userManager.RemoveUserBySessionId(sessionId);
            }
        }


        void OnConnected(ClientSession session)
        {
            _mainLogger.Info(string.Format("세션 번호 {0} 접속", session.SessionID));
        }

        void OnClosed(ClientSession session, CloseReason closeReason)
        {
            _mainLogger.Info(string.Format("세션 번호 {0} 접속해제: {1}", session.SessionID, closeReason.ToString()));

            OnUserClosed(session.SessionID);
        }

        void OnPacketReceived(ClientSession clientSession, MemoryPackBinaryRequestInfo requestInfo)
        {
            // MainLogger.Debug(string.Format("세션 번호 {0} 받은 데이터 크기: {1}, ThreadId: {2}", clientSession.SessionID, requestInfo.Body.Length, System.Threading.Thread.CurrentThread.ManagedThreadId));
            
            requestInfo.SessionID = clientSession.SessionID;
            _mainPacketProcessor.Insert(requestInfo);
        }

    }

    public class ClientSession : AppSession<ClientSession, MemoryPackBinaryRequestInfo>
    {
    }
}
