using GameServer.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class Room
    {
        public int RoomNumber { get; set; }
        public int RoomMaxUserCount { get; set; }
        List<RoomUser> _roomUsers = new List<RoomUser>();

        public void Init(int roomNumber, int roomMaxUserCount)
        {
            RoomNumber = roomNumber;
            RoomMaxUserCount = roomMaxUserCount;
        }


        public ErrorCode AddRoomUser(string uid,  string roomSessionId)
        {
            if(_roomUsers.Count >= RoomMaxUserCount) // 방 정원 초과
            {
                return ErrorCode.ExceedMaxRoomUser;
            }

            if(GetRoomUserBySessionId(uid) != null)
            {
                return ErrorCode.AlreadyExsistUser;
            }

            var roomUser = new RoomUser();
            roomUser.Set(uid, roomSessionId);
            _roomUsers.Add(roomUser);

            Console.WriteLine($"[{RoomNumber}번 room] Uid {uid} 입장, 현재 인원: {_roomUsers.Count}");

            return ErrorCode.None;
        }


        public RoomUser? GetRoomUserBySessionId(string sessionId) 
        {
            return _roomUsers.Find(ru => ru.RoomSessionID == sessionId);
        }

        public ErrorCode RemoveUserBySessionId(string sessioniId)
        {
            RoomUser? roomUser = _roomUsers.Find(ru => ru.RoomSessionID == sessioniId);
            if(roomUser == null)
            {
                return ErrorCode.NullUser;
            }

            if(_roomUsers.Remove(roomUser) == false)
            {
                return ErrorCode.FailRemoveRoomUser;
            }

            Console.WriteLine($"[{RoomNumber}번 room] Uid {roomUser.UserId} 퇴장, 현재 인원: {_roomUsers.Count}");

            return ErrorCode.None;
        }

        public List<RoomUser> GetRoomUserList()
        {
            return _roomUsers;
        }
        

        // 룸에서 방의 모든 사람에게 패킷 전송
        // 유저가 강제종료 시 남아있는 유저에게 패킷을 전송할때만 사용
        public void NotifyRoomUsersFromRoom(Func<string, byte[], bool> NetSendFunc, byte[] sendData)
        {
            foreach(var roomUser in _roomUsers)
            {
                NetSendFunc(roomUser.RoomSessionID, sendData);
            }
        }
    }

    
}
