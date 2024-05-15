using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GameServer;

public class RoomManager
{
    List<Room> rooms = new List<Room>();

    System.Timers.Timer? roomCheckTimer;

    Action<MemoryPackBinaryRequestInfo>? _insertInnerPacket;
    
    double timerWeight = 0.5; // 방 검사 타이머 오차 보정값 네트워크 상황에 따라 조정
    int _roomCheckStartIndex = -1; // 방 검사 시작할 인덱스

    int _roomMaxCount { get; set; } = 0;
    int _roomStartNumber { get; set; } = 0;
    int _roomMaxUserCount { get; set; } = 0;
    int _checkFrequencyRoom { get; set; } = 0;
    int _checkAtOnceRoomCount { get; set; } = 0;

    ConcurrentQueue<int> _playableRoomNumbers;

    public void Init(MainServerOption option, Action<MemoryPackBinaryRequestInfo> InsertInnerPacket, ConcurrentQueue<int> playableRoomNumbers)
    {
        // 지정된 개수만큼 룸 생성후 풀링
        _roomMaxCount = option.RoomMaxCount;
        _roomStartNumber = option.RoomStartNumber;
        _roomMaxUserCount = option.RoomMaxUserCount;
        _checkFrequencyRoom = option.CheckFrequencyRoom;
        _checkAtOnceRoomCount = option.CheckAtOnceRoomCount;

        for (int i = 0; i <= _roomMaxCount; i++)
        {
            var room = new Room();
            room.Init(_roomStartNumber + i, _roomMaxUserCount);
            rooms.Add(room);
        }

        // 룸 체크 타이머 설정
        int awakeUpTime = _checkFrequencyRoom / (_roomMaxCount / _checkAtOnceRoomCount);
        roomCheckTimer = new System.Timers.Timer(awakeUpTime);
        roomCheckTimer.Elapsed += CheckRoomState;
        roomCheckTimer.AutoReset = true;
        roomCheckTimer.Enabled = true;

        _roomCheckStartIndex = 0;

        _insertInnerPacket = InsertInnerPacket;

        _playableRoomNumbers = playableRoomNumbers;
    }

    public List<Room> GetRoomList() { return rooms; }

    public Room? FindRoomByRoomNumber(int roomNumber)
    {
        return rooms.Find(r => r.RoomNumber == roomNumber);
    }

#pragma warning disable 8602
    void CheckRoomState(Object? source, ElapsedEventArgs e)
    {
        for(int i = 0; i < _checkAtOnceRoomCount; i++, _roomCheckStartIndex++)
        {
            if (_roomCheckStartIndex >= rooms.Count)
            {
                _roomCheckStartIndex = 0;
            }

            Room curRoom = rooms[_roomCheckStartIndex];

            if (curRoom.State != RoomState.InGame) // InGame인 상태만 턴 검사
                continue;

            if(CheckTurnTimeout(curRoom) == false)
                continue;

            // 강제로 턴을 넘기는 패킷을 전송한다.
            PKTInnerNtfTurnChange innerSendData = new PKTInnerNtfTurnChange();
            innerSendData.RoomNumber = curRoom.RoomNumber;
            innerSendData.CurTurnUserId = curRoom.CurTurnUserId;
            var innerSendPacket = MemoryPackSerializer.Serialize(innerSendData);
            MemoryPackPacketHeadInfo.Write(innerSendPacket, InnerPacketId.PKTInnerNtfTurnChange);
            _insertInnerPacket(new MemoryPackBinaryRequestInfo(innerSendPacket));

        }
    }
#pragma warning restore 8602

    public bool CheckTurnTimeout(Room room)
    {
        // 마지막으로 턴이 넘어간 시간과 현재 시간을 비교하여 강제로 넘길지 결정
        TimeSpan timeSpan = DateTime.Now - room.lastTurnChangeTime;

        if (timeSpan.TotalSeconds + timerWeight > OmokRule.MaxTurnTime)
        {
            return true;
        }

        return false;
    }


    public ConcurrentQueue<int> GetPlayableRoomNumbers()
    {
        return _playableRoomNumbers;
    }
}

// 방 상태 조사시 InGame인 상태만 조사한다.
public enum RoomState
{
    Full, // 사람이 들어와만 있는 상태(게임 X)
    Empty, // 방에 아무도 없는 상태
    InGame,
}
