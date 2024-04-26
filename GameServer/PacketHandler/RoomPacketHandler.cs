using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.PacketHandler
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
                    Console.WriteLine("[C_RoomEnterReqHandler] Packet Error");
                    return;
                }

                
                // 유저 매니저를 통해 실제 있는 유저인지 체크
                User? user = _userManager.GetUserBySessionId(sessionId);
                
                if(user == null)
                {
                    ResRoomEnter(ErrorCode.NullUser);
                    return;
                }


                // roomNumber를 통해 존재하는 방이면서 2명이하라서 들어갈 수 있는 지 체크
                Room? room = _roomManager.FindRoomByRoomNumber(bodyData.RoomNumber);
                if (room == null)
                {
                    ResRoomEnter(ErrorCode.NullRoom);
                    return;
                }

                ErrorCode result = room.AddRoomUser(user.Id, sessionId);
                if (result != ErrorCode.None)
                {
                    ResRoomEnter(ErrorCode.NullRoom);
                    return;
                }

                // 방 입장 성공
                ResRoomEnter(ErrorCode.None, room, user);
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



        // 방 입장 요청 응답
        public void ResRoomEnter(ErrorCode result, Room? room = null, User? user = null)
        {
            if(result != ErrorCode.None)
            {
                // TODO 실패 응답 처리




            }



            // TODO 성공 응답 처리







        }
    }
}
