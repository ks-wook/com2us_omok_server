using GameServer.Session;
using MemoryPack;
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


        OmokGame _omokGame = new OmokGame(); // 오목 게임 객체


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

            MainServer.MainLogger.Info($"[{RoomNumber}번 room] Uid {uid} 입장, 현재 인원: {_roomUsers.Count}");

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

            MainServer.MainLogger.Info($"[{RoomNumber}번 room] Uid {roomUser.UserId} 퇴장, 현재 인원: {_roomUsers.Count}");

            return ErrorCode.None;
        }

        public List<RoomUser> GetRoomUserList()
        {
            return _roomUsers;
        }
        

        // 룸에서 방의 모든 사람에게 패킷 전송
        public void NotifyRoomUsers<T>(Func<string, byte[], bool> NetSendFunc, T sendData, PACKETID packetId)
        {
            var sendPacket = MemoryPackSerializer.Serialize<T>(sendData);
            MemoryPackPacketHeadInfo.Write(sendPacket, packetId);

            foreach (var roomUser in _roomUsers)
            {
                NetSendFunc(roomUser.RoomSessionID, sendPacket);
            }
        }

        public bool CheckAllUsersReady()
        {
            if(_roomUsers.Count != RoomMaxUserCount)
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
            if(roomUser != null) 
            {
                if (roomUser.IsReady == false)
                {
                    roomUser.IsReady = true;
                    MainServer.MainLogger.Info($"[{roomUser.UserId}] 준비완료");
                    return 1;
                }
                else
                {
                    roomUser.IsReady = false;
                    MainServer.MainLogger.Info($"[{roomUser.UserId}] 준비완료 취소");
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

            _omokGame.StartGame(sendData.BlackUserId, sendData.WhiteUserId);
            MainServer.MainLogger.Info
                ($"RoomNumber: {RoomNumber}, 흑돌: {sendData.BlackUserId}, 백돌: {sendData.WhiteUserId} 게임 시작");


            // 모든 유저에게 오목 게임 시작 패킷 전송
            NotifyRoomUsers<PKTNtfStartOmok>(NetSendFunc, sendData, PACKETID.PKTNtfStartOmok);



            // TODO 타이머 실행








        }

    }

    
}
