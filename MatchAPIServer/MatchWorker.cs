using CloudStructures;
using CloudStructures.Structures;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace APIServer;

public interface IMatchWoker : IDisposable
{
    public void AddUser(string userID);

    public (bool, MatchingCompleteData?) GetCompleteMatching(string userID);
}

public class MatchWoker : IMatchWoker
{
    readonly ILogger<MatchWoker> _logger;

    List<string> _pvpServerAddressList = new();

    RedisList<MatchedPlayers> _redisMatchedPlayerList;
    RedisList<MatchingCompleteData> _redisCompleteList;

    System.Threading.Thread _reqWorker = null;
    ConcurrentQueue<string> _reqQueue = new();

    System.Threading.Thread _completeWorker = null;

    // key는 유저ID
    ConcurrentDictionary<string, MatchingCompleteData> _completeDic = new();

    string _redisAddress = "";
    string _matchListKey = "";
    string _matchCompleteListKey = "";

    RedisConnection _redisConnector;

    public MatchWoker(ILogger<MatchWoker> logger, IOptions<MatchingConfig> matchingConfig, IOptions<PvpServerAddressConfig> pvpServerAddressConfig)
    {
        _logger = logger;

        // 매칭 서버 참조 값 설정
        _redisAddress = matchingConfig.Value.GameRedis;
        _matchListKey = matchingConfig.Value.MatchRequestListKey;
        _matchCompleteListKey = matchingConfig.Value.MatchCompleteListKey;
        _pvpServerAddressList = pvpServerAddressConfig.Value.PvpServerAddresses;

        foreach(string s in _pvpServerAddressList)
        {
            Console.WriteLine(s);
        }

        // Redis 연결
        RedisConfig rc = new RedisConfig("defalut", _redisAddress);
        _redisConnector = new RedisConnection(rc);

        // 소켓 서버 매칭 요청 리스트, 매칭 요청 완료 리스트
        var defalutExpiry = TimeSpan.FromDays(1);
        _redisMatchedPlayerList = new RedisList<MatchedPlayers>(_redisConnector, _matchListKey, defalutExpiry);
        _redisCompleteList = new RedisList<MatchingCompleteData>(_redisConnector, _matchCompleteListKey, defalutExpiry);

        _reqWorker = new System.Threading.Thread(this.RunMatching);
        _reqWorker.Start();

        _completeWorker = new System.Threading.Thread(this.RunMatchingComplete);
        _completeWorker.Start();
    }

    public void AddUser(string userID)
    {
        _reqQueue.Enqueue(userID);
    }

    public (bool, MatchingCompleteData?) GetCompleteMatching(string userID)
    {
        // _completeDic에서 검색해서 있으면 반환한다.
        MatchingCompleteData? matchingCmplData;
        _completeDic.TryRemove(userID, out matchingCmplData);

        if (matchingCmplData == null) // 해당 유저에 대해 매칭 완료된 정보가 없는 경우
        {
            return (false, null);
        }

        return (true, matchingCmplData);
    }

    async void RunMatching()
    {
        while (true)
        {
            try
            {
                await Console.Out.WriteLineAsync($"Matching Req queue size : {_reqQueue.Count}");

                if (_reqQueue.Count < 2)
                {
                    System.Threading.Thread.Sleep(1000);
                    continue;
                }

                // 큐에서 2명을 가져온다. 두명을 매칭시킨다
                string? user1ID, user2ID;
                _reqQueue.TryDequeue(out user1ID);
                _reqQueue.TryDequeue(out user2ID);

                // Redis list 이용해서 매칭된 유저들을 redis 에 전달한다.
                MatchedPlayers matchedData = new MatchedPlayers
                {
                    User1ID = user1ID,
                    User2ID = user2ID
                };

                await _redisMatchedPlayerList.RightPushAsync(matchedData);
            }
            catch (Exception ex)
            {
                _logger.LogError
                    ($"[RunMatching] ErrorCode: {ErrorCode.MatchWorkerException}, {ex.ToString()}");
            }
        }
    }

    async void RunMatchingComplete()
    {
        while (true)
        {
            try
            {
                // Redis의 list를 이용해서 매칭된 결과를 게임서버로부터 받는다
                var matchCmplResult = await _redisCompleteList.LeftPopAsync();
                if(matchCmplResult.HasValue == false)
                {
                    System.Threading.Thread.Sleep(1);
                    continue;
                }

                // 게임 서버는 매칭이 완료된 양쪽의 플레이어 정보를 2번 넣어준다.
                _completeDic.TryAdd(matchCmplResult.Value.UserID, matchCmplResult.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError
                    ($"[RunMatchingComplete] ErrorCode: {ErrorCode.MatchWorkerException}, {ex.ToString()}");
            }
        }
    }

    public void Dispose()
    {
        Console.WriteLine("MatchWoker 소멸자 호출");
    }
}

// 대전서버에서 보내주는 매칭 완료 응답
public class MatchingCompleteData
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public string UserID { get; set; } = string.Empty;
    public bool IsMatched { get; set; } = false;
    public string ServerAddress { get; set; } = "";
    public int Port { get; set; }
    public int RoomNumber { get; set; } = 0;
}

public class PvpServerAddressConfig
{
    public List<string> PvpServerAddresses { get; set; }
}

public class MatchingConfig
{
    public string GameRedis { get; set; } = "";
    public string MatchRequestListKey { get; set; } = "";
    public string MatchCompleteListKey { get; set; } = "";
}

public class MatchedPlayers
{
    public string? User1ID { get; set; } = string.Empty;
    public string? User2ID { get; set; } = string.Empty;
}