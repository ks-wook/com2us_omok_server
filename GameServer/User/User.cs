using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;

    public DateTime lastPingCheckTime; // 마지막으로 핑 응답을 받은 시간

    public UserState State { get; set; } = UserState.Login;
    public int RoomNumber { get; private set; } = -1;

    public User(string id, string sessionId)
    {
        Id = id;
        SessionId = sessionId;

        UpdateLastPingCheckTime();
    }

    public void EnteredRoom(int roomNumber)
    {
        State = UserState.InRoom;
        RoomNumber = roomNumber;
    }

    public void LeavedRoom()
    {
        State = UserState.Login;
        RoomNumber = -1;
    }

    public void UpdateLastPingCheckTime()
    {
        lastPingCheckTime = DateTime.Now;
    }
}

// User Server State
public enum UserState : short
{
    Login = 1001,
    InRoom = 1002,
    InGame = 1003,

    Disconnected = 1004,
}


