using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GameServer;

public class UserManager
{
    int _maxConnectionNumber;

    List<User> users = new List<User>();

    System.Timers.Timer? _userCheckTimer;

    Action<MemoryPackBinaryRequestInfo> _insertInnerPacket;
    Action<string> _closeConnection;

    int userCheckStartIndex = -1; // 유저 검사 시작할 인덱스

    int _checkFrequencyUser { get; set; } = 0;
    int _checkAtOnceUserRatio { get; set; } = 0;
    int _checkAtOnceUserCount { get; set; } = 0;
    double _maxPingDelayTime { get; set; } = 0;

    public void Init(MainServerOption option, Action<MemoryPackBinaryRequestInfo> InsertInnerPacket, Action<string> CloseConnection)
    {
        _maxConnectionNumber = option.MaxConnectionNumber;
        _checkFrequencyUser = option.CheckFrequencyUser;
        _checkAtOnceUserRatio = option.CheckAtOnceUserRatio;
        _maxPingDelayTime = option.MaxPingDelayTime;

        int awakeUpTime = _checkFrequencyUser / _checkAtOnceUserRatio;
        _userCheckTimer = new System.Timers.Timer(awakeUpTime);
        _userCheckTimer.Elapsed += CheckUserState;
        _userCheckTimer.AutoReset = true;
        _userCheckTimer.Enabled = true;

        userCheckStartIndex = 0;

        _insertInnerPacket = InsertInnerPacket;
        _closeConnection = CloseConnection;
    }

    void CheckUserState(Object? source, ElapsedEventArgs e)
    {
        _checkAtOnceUserCount = (users.Count + _checkAtOnceUserRatio - 1) / _checkAtOnceUserRatio;

        for (int i = 0; i < _checkAtOnceUserCount; i++, userCheckStartIndex++)
        {
            if (userCheckStartIndex >= users.Count) // 끝까지 검사 완료
            {
                userCheckStartIndex = 0;
                break;
            }

            User? curUser = users.ElementAtOrDefault(userCheckStartIndex);

            if (curUser == null)
                continue;

            // 핑 성공 시에 다시 핑요청을 보내고
            // 핑 지연시간이 초과한 경우 끊어낸다.
            if(CheckPingTimeout(curUser) == true)
            {
                // 이 유저를 끊어내라는 요청을 보낸다.
                PKTInnerNtfCloseConnection ntfCloseConnection = new PKTInnerNtfCloseConnection();
                ntfCloseConnection.SessionId = curUser.SessionId;
                var ntfCloseConnectionPacket = MemoryPackSerializer.Serialize(ntfCloseConnection);
                MemoryPackPacketHeadInfo.Write(ntfCloseConnectionPacket, InnerPacketId.PKTInnerNtfCloseConnection);
                _insertInnerPacket(new MemoryPackBinaryRequestInfo(ntfCloseConnectionPacket));

                continue;
            }

            // 메인 프로세서로 해당 유저에게 핑 요청을 할 것을 전달한다.
            PKTInnerNtfSendPing innerSendData = new PKTInnerNtfSendPing();
            innerSendData.SessionId = curUser.SessionId;
            var innerSendPacket = MemoryPackSerializer.Serialize(innerSendData);
            MemoryPackPacketHeadInfo.Write(innerSendPacket, InnerPacketId.PKTInnerNtfSendPing);
            _insertInnerPacket(new MemoryPackBinaryRequestInfo(innerSendPacket));
        }

    }

    public bool CheckPingTimeout(User user)
    {
        // 마지막으로 핑이 도착한 시간과 현재 시간을 비교하여 강제로 넘길지 결정
        TimeSpan timeSpan = DateTime.Now - user.lastPingCheckTime;
        // MainServer.MainLogger.Info($"ping : {timeSpan.TotalMilliseconds}  {user.Id} 시간차");

        if (timeSpan.TotalSeconds > _maxPingDelayTime + 0.5)
        {
            return true;
        }

        return false;
    }

    public ErrorCode AddUser(string userId, string SessionId)
    {
        if (CheckMaxConnection() == false)
        {
            return ErrorCode.ExceedMaxUserConnection;
        }

        if (CheckExistUser(userId) == false)
        {
            return ErrorCode.AlreadyExsistUser;
        }

        users.Add(new User(userId, SessionId));
        return ErrorCode.None;
    }

    public ErrorCode RemoveUserBySessionId(string sessionId)
    {
        User? user = users.Find(u => u.SessionId == sessionId);
        if (user == null)
        {
            return ErrorCode.NullUser;
        }

        users.Remove(user);

        return ErrorCode.None;
    }

    bool CheckMaxConnection()
    {
        if (users.Count < _maxConnectionNumber) { return true; }

        return false;
    }

    bool CheckExistUser(string userId)
    {
        if (users.Find(u => u.Id == userId) != null) // 이미 접속한 유저
        {
            return false;
        }

        return true;
    }

    public User? GetUserBySessionId(string SessionId)
    {
        return users.Find(u => u.SessionId == SessionId);
    }

    public User? GetUserByUID(string uid)
    {
        return users.Find(u => uid == u.Id);
    }

    public void CloseConnectionByUserId(string userId)
    {
        User? user = GetUserByUID(userId);
        if (user != null)
        {
            _closeConnection(user.SessionId); // 연결 종료
            users.Remove(user); // 유저 목록에서 삭제
        }
    }

    public void CloseConnectionBySessionId(string sessionId)
    { 
        _closeConnection(sessionId);
    }
}
