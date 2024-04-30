using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class PacketHandlerRoom : PacketHandler
    {
        RoomManager _roomManager;
        UserManager _userManager;


        public PacketHandlerRoom(RoomManager? roomManager, UserManager? userManger)
        {
            if(roomManager == null || userManger == null)
            {
                Console.WriteLine("[RoomPacketHandler.Init] roomList null");
                throw new NullReferenceException();
            }

            this._roomManager = roomManager;
            this._userManager = userManger;
        }


        public override void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap)
        {
            // 방 입퇴장
            packetHandlerMap.Add((int)PACKETID.PKTReqRoomEnter, C_RoomEnterReqHandler);
            packetHandlerMap.Add((int)PACKETID.PKTReqRoomLeave, C_LeaveRoomReqHandler);
            packetHandlerMap.Add((int)PACKETID.PKTReqRoomChat, C_RoomChatHandler);


            // 오목
            packetHandlerMap.Add((int)PACKETID.PKTReqReadyOmok, C_ReadyOmokHandler);
            packetHandlerMap.Add((int)PACKETID.PKTReqPutMok, C_PutMokHandler);
        }

       
        // 룸의 모든 유저에게 패킷 전송
        public void NotifyRoomUsers(byte[] sendData, Room room)
        {
            
            foreach(RoomUser roomUser in room.GetRoomUserList())
            {
                NetSendFunc(roomUser.RoomSessionID, sendData);
            }
        }

        // 오목 게임 시작 통보 패킷 전송
        public void OmokStart(Room room)
        {
            // 모든 유저에게 오목 게임 시작 패킷 전송
            List<RoomUser> roomUsers = room.GetRoomUserList();

            // 선후공 결정
            int blackUser = RandomNumberGenerator.GetInt32(2);
            PKTNtfStartOmok sendData = new PKTNtfStartOmok();

            if(blackUser == 0)
            {
                sendData.BlackUserId = roomUsers[0].UserId;
                sendData.WhiteUserId = roomUsers[1].UserId;
            }
            else
            {
                sendData.BlackUserId = roomUsers[1].UserId;
                sendData.WhiteUserId = roomUsers[0].UserId;
            }

            room.OmokGameStart(sendData.BlackUserId, sendData.WhiteUserId);


            // 오목 시작 패킷 전달
            var sendPacket = MemoryPackSerializer.Serialize(sendData);
            MemoryPackPacketHeadInfo.Write(sendPacket, PACKETID.PKTNtfStartOmok);
            NotifyRoomUsers(sendPacket, room);
        }

        // 방 입장 요청
        public void C_RoomEnterReqHandler(MemoryPackBinaryRequestInfo packet)
        {
            var sessionId = packet.SessionID;

            try
            {
                var bodyData = MemoryPackSerializer.Deserialize<PKTReqRoomEnter>(packet.Data);

                if(bodyData == null)
                {
                    Console.WriteLine($"[C_RoomEnterReqHandler] ErrorCode: {ErrorCode.NullPacket}");
                    return;
                }

                
                // 유저 매니저를 통해 실제 있는 유저인지 체크
                User? user = _userManager.GetUserBySessionId(sessionId);
                
                if(user == null)
                {
                    Console.WriteLine($"[C_RoomEnterReqHandler] ErrorCode: {ErrorCode.NullUser}");
                    return;
                }


                // roomNumber를 통해 존재하는 방이면서 2명이하라서 들어갈 수 있는 지 체크
                Room? room = _roomManager.FindRoomByRoomNumber(bodyData.RoomNumber);
                if (room == null)
                {
                    Console.WriteLine($"[C_RoomEnterReqHandler] ErrorCode: {ErrorCode.NullRoom}");
                    return;
                }

                ErrorCode result = room.AddRoomUser(user.Id, sessionId);
                if (result != ErrorCode.None)
                {
                    Console.WriteLine($"[C_RoomEnterReqHandler] ErrorCode: {result}");
                    return;
                }



                // 방 입장 성공 응답
                {
                    PKTResRoomEnter sendData = new PKTResRoomEnter();
                    sendData.UserId = user.Id;
                    sendData.RoomNumber = bodyData.RoomNumber;
                    var sendPacket = MemoryPackSerializer.Serialize<PKTResRoomEnter>(sendData);
                    MemoryPackPacketHeadInfo.Write(sendPacket, PACKETID.PKTResRoomEnter);
                    NotifyRoomUsers(sendPacket, room);
                }

                // 서버 유저 상태 변경
                user.EnteredRoom(bodyData.RoomNumber);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[C_RoomEnterReqHandler] Packet Error " + ex.ToString());
            }

        }
        

        // 방 퇴장 요청
        public void C_LeaveRoomReqHandler(MemoryPackBinaryRequestInfo packet)
        {
            var sessionId = packet.SessionID;

            try
            {
                User? user = _userManager.GetUserBySessionId(sessionId);
                if (user == null)
                {
                    Console.WriteLine($"[C_LeaveRoomReqHandler] ErrorCode: {ErrorCode.NullUser}");
                    return;
                }

                // 유저가 방에 입장한 상태인가?
                if (user.State != UserState.InRoom)
                {
                    Console.WriteLine($"[C_LeaveRoomReqHandler] ErrorCode: {ErrorCode.InvalidRequest}");
                    return;
                }



                // Room을 찾고 룸 유저 삭제
                Room? room = _roomManager.FindRoomByRoomNumber(user.RoomNumber);
                if (room == null)
                {
                    Console.WriteLine($"[C_LeaveRoomReqHandler] ErrorCode: {ErrorCode.NullRoom}");
                    return;
                }


                ErrorCode result = room.RemoveUserBySessionId(sessionId);
                if(result != ErrorCode.None)
                {
                    Console.WriteLine($"[C_LeaveRoomReqHandler] ErrorCode: {result}");
                    return;
                }



                // 방 퇴장 성공 응답
                {
                    PKTResRoomLeave sendData = new PKTResRoomLeave();
                    sendData.UserId = user.Id;
                    var sendPacket = MemoryPackSerializer.Serialize(sendData);
                    MemoryPackPacketHeadInfo.Write(sendPacket, PACKETID.PKTResRoomLeave);
                    NotifyRoomUsers(sendPacket, room);

                    // 방을 나간 유저에게는 따로 보낸다.
                    NetSendFunc(sessionId, sendPacket);
                }

                // 서버 유저 상태 변경
                user.LeavedRoom();

            }
            catch(Exception ex)
            {
                Console.WriteLine($"[C_LeaveRoomReqHandler] ErrorCode: {ErrorCode.LeaveRoomFail}"); ;
            }

        }


        // 방 채팅
        public void C_RoomChatHandler(MemoryPackBinaryRequestInfo packet)
        {
            var sessionId = packet.SessionID;

            try
            {
                var bodyData = MemoryPackSerializer.Deserialize<PKTReqRoomChat>(packet.Data);

                if (bodyData == null)
                {
                    Console.WriteLine($"[C_RoomChatHandler] ErrorCode: {ErrorCode.NullPacket}");
                    return;
                }


                User? user = _userManager.GetUserBySessionId(sessionId);
                if (user == null)
                {
                    Console.WriteLine($"[C_RoomChatHandler] ErrorCode: {ErrorCode.NullUser}");
                    return;
                }

                // 유저가 방에 입장한 상태인가?
                if (user.State != UserState.InRoom)
                {
                    Console.WriteLine($"[C_RoomChatHandler] ErrorCode: {ErrorCode.InvalidRequest}");
                    return;
                }


                // Room을 찾고 room 내의 유저들에게 채팅 Notify
                Room? room = _roomManager.FindRoomByRoomNumber(user.RoomNumber);
                if (room == null)
                {
                    Console.WriteLine($"[C_RoomChatHandler] ErrorCode: {ErrorCode.NullRoom}");
                    return;
                }


                // 채팅 메시지 전달
                {
                    PKTResRoomChat sendData = new PKTResRoomChat();
                    sendData.UserId = user.Id;
                    sendData.ChatMsg = bodyData.ChatMsg;
                    var sendPacket = MemoryPackSerializer.Serialize(sendData);
                    MemoryPackPacketHeadInfo.Write(sendPacket, PACKETID.PKTResRoomChat);
                    NotifyRoomUsers(sendPacket, room);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[C_RoomChatHandler] ErrorCode: {ErrorCode.RoomChatFail}"); ;
            }


        }

        
        // 오목 준비 완료 요청
        public void C_ReadyOmokHandler(MemoryPackBinaryRequestInfo packet)
        {
            var sessionId = packet.SessionID;

            try
            {
                User? user = _userManager.GetUserBySessionId(sessionId);
                if (user == null)
                {
                    Console.WriteLine($"[C_ReadyOmokHandler] ErrorCode: {ErrorCode.NullUser}");
                    return;
                }

                // 유저가 방에 입장한 상태인가?
                if (user.State != UserState.InRoom)
                {
                    Console.WriteLine($"[C_ReadyOmokHandler] ErrorCode: {ErrorCode.InvalidRequest}");
                    return;
                }


                // Room을 찾고 roomUser의 준비 상태 변경
                Room? room = _roomManager.FindRoomByRoomNumber(user.RoomNumber);
                if (room == null)
                {
                    Console.WriteLine($"[C_ReadyOmokHandler] ErrorCode: {ErrorCode.NullRoom}");
                    return;
                }

                int result = room.ChangeIsReadyBySessionId(sessionId);
                if(result == -1 )
                {
                    Console.WriteLine($"[C_ReadyOmokHandler] ErrorCode: {ErrorCode.NullUser}");
                    return;
                }

                // 준비상태 변경 응답
                {
                    PKTResReadyOmok sendData = new PKTResReadyOmok();
                    sendData.UserId = user.Id;

                    if (result == 0)
                    {
                        sendData.IsReady = false;

                    }
                    else if(result == 1)
                    {
                        sendData.IsReady = true;
                    }

                    var sendPacket = MemoryPackSerializer.Serialize(sendData);
                    MemoryPackPacketHeadInfo.Write(sendPacket, PACKETID.PKTResReadyOmok);
                    NotifyRoomUsers(sendPacket, room);

                }

                // 모든 유저의 준비가 끝났는지 검사
                if(room.CheckAllUsersReady())
                {
                    OmokStart(room);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"[C_ReadyOmokHandler] ErrorCode: {ErrorCode.FailReadyOmok}"); ;
            }
        }

        // 오목 돌을 두는 요청
        public void C_PutMokHandler(MemoryPackBinaryRequestInfo packet)
        {
            var sessionId = packet.SessionID;

            try
            {
                var bodyData = MemoryPackSerializer.Deserialize<PKTReqPutMok>(packet.Data);

                if (bodyData == null)
                {
                    Console.WriteLine($"[C_PutMokHandler] ErrorCode: {ErrorCode.NullPacket}");
                    return;
                }

                User? user = _userManager.GetUserBySessionId(sessionId);
                if (user == null)
                {
                    Console.WriteLine($"[C_PutMokHandler] ErrorCode: {ErrorCode.NullUser}");
                    return;
                }

                // Room을 찾고 서버 오목판에 돌을 반영
                Room? room = _roomManager.FindRoomByRoomNumber(user.RoomNumber);
                if (room == null)
                {
                    Console.WriteLine($"[C_PutMokHandler] ErrorCode: {ErrorCode.NullRoom}");
                    return;
                }

                room.GetOmokGame().PlaceStone(bodyData.PosX, bodyData.PosY, user.Id);

                // 돌을 두었다는 완료 패킷 전송
                {
                    PKTResPutMok sendData = new PKTResPutMok();
                    sendData.UserId = user.Id;
                    sendData.PosX = bodyData.PosX;
                    sendData.PosY = bodyData.PosY;
                    var sendPacket = MemoryPackSerializer.Serialize(sendData);
                    MemoryPackPacketHeadInfo.Write(sendPacket, PACKETID.PKTResPutMok);
                    NotifyRoomUsers(sendPacket, room);
                }


                // 승부가 났다면 게임 종료 패킷도 전송
                if (room.GetOmokGame().CheckWinner(bodyData.PosX, bodyData.PosY)) 
                {
                    PKTNtfEndOmok sendData = new PKTNtfEndOmok();
                    sendData.WinUserId = user.Id;
                    var sendPacket = MemoryPackSerializer.Serialize(sendData);
                    MemoryPackPacketHeadInfo.Write(sendPacket, PACKETID.PKTNtfEndOmok);
                    NotifyRoomUsers(sendPacket, room);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[C_PutMokHandler] ErrorCode: {ErrorCode.PutMokFail}"); ;
            }

        }
    }
}
