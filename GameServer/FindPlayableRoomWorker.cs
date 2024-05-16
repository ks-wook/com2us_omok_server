using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Logging;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class FindPlayableRoomWorker : IDisposable
{
    ILog _logger;

    // 플레이 가능한 룸 넘버들
    ConcurrentQueue<int> _playableRoomNumbers;

    RedisList<MatchedPlayers> _redisMatchedPlayerList;
    RedisList<MatchingCompleteData> _redisCompleteList;

    Thread _findPlayableRoomWorkerThread;
    bool IsThreadRunning = false;

    string _redisAddress = "";
    string _matchListKey = "";
    string _matchCompleteListKey = "";

    RedisConnection _redisConnector;

    string _pvpServerAddress;
    int _pvpServerPort;

    public FindPlayableRoomWorker(ILog logger, ConcurrentQueue<int> playableRoomNumbers, string redisAddress, string matchRequestListKey, string matchCompleteListKey, string pvpServerAddress, int pvpServerPort)
    {
        _logger = logger;

        if (playableRoomNumbers == null)
        {
            _logger.Error($"[FindPlayableRoomWorker] playableRoomNumbers null");
            throw new ArgumentNullException();
        }

        _playableRoomNumbers = playableRoomNumbers;

        // redis 연결 초기화
        _matchListKey = matchRequestListKey;
        _matchCompleteListKey = matchCompleteListKey;

        RedisConfig rc = new RedisConfig("defalut", redisAddress);
        _redisConnector = new RedisConnection(rc);

        var defalutExpiry = TimeSpan.FromDays(1);
        _redisMatchedPlayerList = new RedisList<MatchedPlayers>(_redisConnector, _matchListKey, defalutExpiry);
        _redisCompleteList = new RedisList<MatchingCompleteData>(_redisConnector, _matchCompleteListKey, defalutExpiry);

        IsThreadRunning = true;

        _findPlayableRoomWorkerThread = new Thread(this.RunFindPlayableRoom);
        _findPlayableRoomWorkerThread.Start();

        _pvpServerAddress = pvpServerAddress;
        _pvpServerPort = pvpServerPort;
    }

    public void Dispose()
    {
        IsThreadRunning = false;
    }

    void RunFindPlayableRoom()
    {
        while(IsThreadRunning)
        {
            // redis 에서 값을 빼내기 전에 남은 방이 있는 지 검사한다.
            if(_playableRoomNumbers.Count == 0) // 남은 방이 없다면
            {
                // 잠시 대기 후에 다시 돌아간다.
                Thread.Sleep(1000);
                continue;
            }

            _logger.Debug($"[RunFindPlayableRoom] 현재 대전 가능한 방 수 : {_playableRoomNumbers.Count}");

            var matchReq = _redisMatchedPlayerList.LeftPopAsync().Result;

            if (matchReq.HasValue == false) // 매칭 완료된 유저 정보가 없는 경우
            {
                Thread.Sleep(1000);
                continue;
            }

            // 플레이 가능한 방을 가져온다.
            int reservedRoomNum = -1;
            bool result = _playableRoomNumbers.TryDequeue(out reservedRoomNum);

            if (result == false)
            {
                // 잠시 대기 후에 다시 돌아간다.
                Thread.Sleep(1000);
                continue;
            }

            _logger.Debug($"[RunFindPlayableRoom] 매칭 완료 수신 방 배정 : {reservedRoomNum}");

            // 플레이 가능한 방 정보를 redis 에 넣는다.
            _ = _redisCompleteList.RightPushAsync(new MatchingCompleteData
            {
                UserID = matchReq.Value.User1ID,
                IsMatched = true,
                ServerAddress = _pvpServerAddress,
                Port = _pvpServerPort,
                RoomNumber = reservedRoomNum
            }).Result;

            _ = _redisCompleteList.RightPushAsync(new MatchingCompleteData
            {
                UserID = matchReq.Value.User2ID,
                IsMatched = true,
                ServerAddress = _pvpServerAddress,
                Port = _pvpServerPort,
                RoomNumber = reservedRoomNum
            }).Result;
        }

    }
}

public class MatchingCompleteData
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public string UserID { get; set; } = string.Empty;
    public bool IsMatched { get; set; } = false;
    public string ServerAddress { get; set; } = "";
    public int Port { get; set; }
    public int RoomNumber { get; set; } = 0;
}

public class MatchedPlayers
{
    public string User1ID { get; set; } = string.Empty;
    public string User2ID { get; set; } = string.Empty;
}