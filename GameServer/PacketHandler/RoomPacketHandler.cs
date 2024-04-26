﻿using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class RoomPacketHandler : PacketHandler
    {
        RoomManager? _roomManager;
        UserManager? _userManager;

        public void Init(RoomManager roomManager, UserManager userManger)
        {
            if(roomManager == null || userManger == null)
            {
                Console.WriteLine("[RoomPacketHandler.Init] roomList null");
                return;
            }

            _roomManager = roomManager;
            _userManager = userManger;
        }


        public override void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap)
        {
            packetHandlerMap.Add((int)PACKET_ID.C_EnterRoomReq, C_RoomEnterReqHandler);
            packetHandlerMap.Add((int)PACKET_ID.C_LeaveRoomReq, C_LeaveRoomReqHandler);
            packetHandlerMap.Add((int)PACKET_ID.C_RoomChat, C_RoomChatHandler);
        }



        // 방 입장 요청
        public void C_RoomEnterReqHandler(MemoryPackBinaryRequestInfo packet)
        {
            var sessionId = packet.SessionID;

            try
            {
                var bodyData = MemoryPackSerializer.Deserialize<C_EnterRoomReq>(packet.Data);

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

                // 방 입장 성공
                // TODO 방 입장 성공 패킷 전송
                // ResRoomEnter(ErrorCode.None, room, user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[C_RoomEnterReqHandler] Packet Error " + ex.ToString());
            }

        }
        

        // 방 퇴장 요청
        public void C_LeaveRoomReqHandler(MemoryPackBinaryRequestInfo packet)
        {

        }


        // 방 채팅
        public void C_RoomChatHandler(MemoryPackBinaryRequestInfo packet)
        {

        }


    }
}
