using GameServer.Packet;
using MemoryPack;
using System;
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

    Action<MemoryPackBinaryRequestInfo> _insertInnerPacket;
    
    double timerWeight = 0.5; // 방 검사 타이머 오차 보정값
    int roomCheckStartIndex = -1; // 방 검사 시작할 인덱스

    private int RoomMaxCount { get; set; } = 0;
    private int RoomStartNumber { get; set; } = 0;
    private int RoomMaxUserCount { get; set; } = 0;
    private int CheckFrequencyRoom { get; set; } = 0;
    private int CheckAtOnceRoomCount { get; set; } = 0;


    public void Init(MainServerOption option, Action<MemoryPackBinaryRequestInfo> InsertInnerPacket)
    {
        // 지정된 개수만큼 룸 생성후 풀링
        RoomMaxCount = option.RoomMaxCount;
        RoomStartNumber = option.RoomStartNumber;
        RoomMaxUserCount = option.RoomMaxUserCount;
        CheckFrequencyRoom = option.CheckFrequencyRoom;
        CheckAtOnceRoomCount = option.CheckAtOnceRoomCount;

        for (int i = 0; i < RoomMaxCount; i++)
        {
            var room = new Room();
            room.Init(RoomStartNumber + i, RoomMaxUserCount);
            rooms.Add(room);
        }

        // 룸 체크 타이머 설정
        int awakeUpTime = CheckFrequencyRoom / (RoomMaxCount / CheckAtOnceRoomCount);
        roomCheckTimer = new System.Timers.Timer(awakeUpTime);
        roomCheckTimer.Elapsed += CheckRoomState;
        roomCheckTimer.AutoReset = true;
        roomCheckTimer.Enabled = true;

        roomCheckStartIndex = 0;

        this._insertInnerPacket = InsertInnerPacket;
    }

    public List<Room> GetRoomList() { return rooms; }


    public Room? FindRoomByRoomNumber(int roomNumber)
    {
        return rooms.Find(r => r.RoomNumber == roomNumber);
    }


    void CheckRoomState(Object? source, ElapsedEventArgs e)
    {
        for(int i = 0; i < CheckAtOnceRoomCount; i++, roomCheckStartIndex++)
        {

            if (roomCheckStartIndex >= rooms.Count)
            {
                roomCheckStartIndex = 0;
            }

            Room curRoom = rooms[roomCheckStartIndex];

            if (curRoom.State == RoomState.None)
                continue;


            if(CheckTurnTimeout(curRoom) == false)
                continue;


            // 강제로 턴을 넘기는 패킷을 전송한다.
            PKTInnerNtfTurnChange innerSendData = new PKTInnerNtfTurnChange();
            innerSendData.RoomNumber = curRoom.RoomNumber;
            var innerSendPacket = MemoryPackSerializer.Serialize(innerSendData);
            MemoryPackPacketHeadInfo.Write(innerSendPacket, InnerPacketId.PKTInnerNtfTurnChange);
            _insertInnerPacket(new MemoryPackBinaryRequestInfo(innerSendPacket));

        }

    }


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

}


// 방 상태 조사시 InGame인 상태만 조사한다.
public enum RoomState
{
    None,
    InGame,
}
