using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string SessionId {  get; set; } = string.Empty;
    public UserState State { get; set; } = UserState.Login;
    public int RoomNumber { get; private set; } = -1;

    public User(string id, string sessionId)
    {
        Id = id;
        SessionId = sessionId;
    }

    public void EnteredRoom(int roomNumber)
    {
        this.State = UserState.InRoom;
        RoomNumber = roomNumber;
    }

    public void LeavedRoom()
    {
        this.State = UserState.Login;
        RoomNumber = -1;
    }
}

// User Server State
public enum UserState : short
{
    Login = 1001,
    InRoom = 1002,
    InGame = 1003,



}


