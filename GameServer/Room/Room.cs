using MemoryPack;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class Room
{
    public RoomState State { get; set; } = RoomState.None;

    public DateTime lastTurnChangeTime;
    public string CurTurnUserId { get; set; } = string.Empty; // 현재 턴인 유저 아이디

    public int RoomNumber { get; set; }
    public int RoomMaxUserCount { get; set; }
    List<RoomUser> _roomUsers = new List<RoomUser>();

    OmokGame _omokGame = new OmokGame(); // 오목 게임 객체

    public void Init(int roomNumber, int roomMaxUserCount)
    {
        RoomNumber = roomNumber;
        RoomMaxUserCount = roomMaxUserCount;
    }

    public ErrorCode AddRoomUser(string uid, string roomSessionId)
    {
        if (_roomUsers.Count >= RoomMaxUserCount) // 방 정원 초과
        {
            return ErrorCode.ExceedMaxRoomUser;
        }

        if (GetRoomUserBySessionId(uid) != null)
        {
            return ErrorCode.AlreadyExsistUser;
        }

        var roomUser = new RoomUser();
        roomUser.Set(uid, roomSessionId);
        _roomUsers.Add(roomUser);

        return ErrorCode.None;
    }

    public RoomUser? GetRoomUserBySessionId(string sessionId)
    {
        return _roomUsers.Find(ru => ru.RoomSessionID == sessionId);
    }

    public ErrorCode RemoveUserBySessionId(string sessioniId)
    {
        RoomUser? roomUser = _roomUsers.Find(ru => ru.RoomSessionID == sessioniId);
        if (roomUser == null)
        {
            return ErrorCode.NullUser;
        }

        if (_roomUsers.Remove(roomUser) == false)
        {
            return ErrorCode.FailRemoveRoomUser;
        }

        return ErrorCode.None;
    }

    public List<RoomUser> GetRoomUserList()
    {
        return _roomUsers;
    }

    // 룸에서 방의 모든 사람에게 패킷 전송
    public void NotifyRoomUsers<T>(Func<string, byte[], bool> NetSendFunc, T sendData, PACKETID packetId)
    {
        var sendPacket = MemoryPackSerializer.Serialize(sendData);
        MemoryPackPacketHeadInfo.Write(sendPacket, packetId);

        foreach (var roomUser in _roomUsers)
        {
            NetSendFunc(roomUser.RoomSessionID, sendPacket);
        }
    }

    public bool CheckAllUsersReady()
    {
        if (_roomUsers.Count != RoomMaxUserCount)
        {
            return false;
        }

        foreach (var roomUser in _roomUsers)
        {
            if (roomUser.IsReady == false)
            {
                return false;
            }
        }

        return true;
    }

    // 유저 준비 상태 변경
    public int ChangeIsReadyBySessionId(string sessionId)
    {
        RoomUser? roomUser = GetRoomUserBySessionId(sessionId);
        if (roomUser != null)
        {
            if (roomUser.IsReady == false)
            {
                roomUser.IsReady = true;
                return 1;
            }
            else
            {
                roomUser.IsReady = false;
                return 0;
            }
        }
        return -1;
    }

    // 오목 게임 획득
    public OmokGame GetOmokGame() { return _omokGame; }

    // 오목 게임 시작
    public void OmokGameStart(Func<string, byte[], bool> NetSendFunc)
    {
        // 선후공 결정
        int blackUser = RandomNumberGenerator.GetInt32(2);
        PKTNtfStartOmok sendData = new PKTNtfStartOmok();

        if (blackUser == 0)
        {
            sendData.BlackUserId = _roomUsers[0].UserId;
            sendData.WhiteUserId = _roomUsers[1].UserId;
        }
        else
        {
            sendData.BlackUserId = _roomUsers[1].UserId;
            sendData.WhiteUserId = _roomUsers[0].UserId;
        }

        UpdateLastTurnChangeTime();
        CurTurnUserId = sendData.BlackUserId;

        _omokGame.StartGame(sendData.BlackUserId, sendData.WhiteUserId);

        // 모든 유저에게 오목 게임 시작 패킷 전송
        NotifyRoomUsers(NetSendFunc, sendData, PACKETID.PKTNtfStartOmok);

        State = RoomState.InGame; // 게임 상태 전환
    }

    public void OmokGameEnd()
    {
        State = RoomState.None; // 게임 종료
        
        foreach(RoomUser roomUser in _roomUsers)
        {
            roomUser.IsReady = false;
        }
    }

    // 턴 변경 시간 갱신
    public void UpdateLastTurnChangeTime()
    {
        lastTurnChangeTime = DateTime.Now;
        UpdateCurTurnUser(); 
    }

    // 현재 턴인 유저 아이디 갱신
    void UpdateCurTurnUser()
    {
        if(CurTurnUserId == _omokGame.BlackUserId)
        {
            CurTurnUserId = _omokGame.WhiteUserId;
        }
        else
        {
            CurTurnUserId = _omokGame.BlackUserId;
        }
    }

    public bool CheckCurrentTurnUserId(string userId)
    {
        if(string.CompareOrdinal(CurTurnUserId, userId) != 0)
        {
            return false;
        }

        return true;
    }
}
