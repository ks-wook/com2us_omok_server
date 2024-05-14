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
    RedisList<CompleteMatchingData> _redisCompleteList;

    Thread _findPlayableRoomWorkerThread;
    bool IsThreadRunning = false;

    string _redisAddress = "";
    string _matchListKey = "";
    string _matchCompleteListKey = "";

    RedisConnection _redisConnector;

    string _pvpServerAddress;

    public FindPlayableRoomWorker(ILog logger, ConcurrentQueue<int> playableRoomNumbers, string redisAddress, string matchRequestListKey, string matchCompleteListKey, string pvpServerAddress)
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
        _redisCompleteList = new RedisList<CompleteMatchingData>(_redisConnector, _matchCompleteListKey, defalutExpiry);

        IsThreadRunning = true;

        _findPlayableRoomWorkerThread = new Thread(this.RunFindPlayableRoom);
        _findPlayableRoomWorkerThread.Start();

        _pvpServerAddress = pvpServerAddress;
    }

    public void Dispose()
    {
        IsThreadRunning = false;
    }

    async void RunFindPlayableRoom()
    {
        while(IsThreadRunning)
        {
            var matchReq = await _redisMatchedPlayerList.LeftPopAsync();


            if(matchReq.HasValue == false) // 매칭 완료된 유저 정보가 없는 경우
            {
                Thread.Sleep(1000);
                continue;
            }

            int reservedRoomNum = -1;
            bool result = _playableRoomNumbers.TryDequeue(out reservedRoomNum);

            await Console.Out.WriteLineAsync($"매칭 완료 수신 방 배정 : {reservedRoomNum}");

            if (result == false) // 남은 방이 없다면
            {
                // redis에 매칭된 유저 정보를 다시 넣는다.
                await _redisMatchedPlayerList.RightPushAsync(matchReq.Value);
                continue;
            }

            // 플레이 가능한 방 정보를 리스트에 넣는다.
            await _redisCompleteList.RightPushAsync(new CompleteMatchingData
            {
                UserID = matchReq.Value.User1ID,
                ServerAddress = _pvpServerAddress,
                RoomNumber = reservedRoomNum
            });

            await _redisCompleteList.RightPushAsync(new CompleteMatchingData
            {
                UserID = matchReq.Value.User2ID,
                ServerAddress = _pvpServerAddress,
                RoomNumber = reservedRoomNum
            });
        }

    }
}

public class CompleteMatchingData
{
    public string UserID { get; set; } = "";
    public int Port { get; set; }
    public string ServerAddress { get; set; } = "";
    public int RoomNumber { get; set; } = 0;
}

public class MatchedPlayers
{
    public string User1ID { get; set; } = string.Empty;
    public string User2ID { get; set; } = string.Empty;
}