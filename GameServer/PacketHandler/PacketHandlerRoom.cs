using GameServer.Packet;
using MemoryPack;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class PacketHandlerRoom : BasePacketHandler
{
    ILog _logger;

    RoomManager _roomManager;
    UserManager _userManager;

    public PacketHandlerRoom(ILog logger, RoomManager? roomManager, UserManager? userManger)
    {
        _logger = logger;

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
        packetHandlerMap.Add((int)PACKETID.PKTReqRoomEnter, PKTReqRoomEnterHandler);
        packetHandlerMap.Add((int)PACKETID.PKTReqRoomLeave, PKTReqRoomLeaveHandler);
        packetHandlerMap.Add((int)PACKETID.PKTReqRoomChat, PKTReqRoomChatHandler);
    }
   
    // 방 입장 요청
    public void PKTReqRoomEnterHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        (ErrorCode result, PKTReqRoomEnter? bodyData) = DeserializeNullablePacket<PKTReqRoomEnter>(packet.Data);

        if (result != ErrorCode.None || bodyData == null)
        {
            SendFailPacket<PKTResRoomEnter>(PACKETID.PKTResRoomEnter, sessionId, result);
            return;
        }


        result = EnterRoom(bodyData, sessionId);
        if(result != ErrorCode.None)
        {
            SendFailPacket<PKTResRoomEnter>(PACKETID.PKTResRoomEnter, sessionId, result);
        }
    }
    
    // 방 퇴장 요청
    public void PKTReqRoomLeaveHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        (ErrorCode result, PKTReqRoomLeave? bodyData) = DeserializeNullablePacket<PKTReqRoomLeave>(packet.Data);
        if(result != ErrorCode.None || bodyData == null)
        {
            SendFailPacket<PKTResRoomLeave>(PACKETID.PKTResRoomLeave, sessionId, result);
            return;
        }

        result = LeaveRoom(bodyData, sessionId);
        if(result != ErrorCode.None)
        {
            SendFailPacket<PKTResRoomLeave>(PACKETID.PKTResRoomLeave, sessionId, result);
        }
    }

    // 방 채팅
    public void PKTReqRoomChatHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        (ErrorCode result, PKTReqRoomChat? bodyData) = DeserializeNullablePacket<PKTReqRoomChat>(packet.Data);
        if (result != ErrorCode.None || bodyData == null)
        {
            SendFailPacket<PKTResRoomChat>(PACKETID.PKTResRoomChat, sessionId, result);
            return;
        }

        result = ChatRoom(bodyData, sessionId);
        if (result != ErrorCode.None)
        {
            SendFailPacket<PKTResRoomChat>(PACKETID.PKTResRoomChat, sessionId, result);
        }

    }
    public ErrorCode EnterRoom(PKTReqRoomEnter packet, string sessionId)
    {
        User? user = _userManager.GetUserBySessionId(sessionId);
        if (user == null)
        {
            Console.WriteLine($"[EnterRoom] ErrorCode: {ErrorCode.NullUser}");
            return ErrorCode.NullUser;
        }

        // roomNumber를 통해 존재하는 방이면서 2명이하라서 들어갈 수 있는 지 체크
        Room? room = _roomManager.FindRoomByRoomNumber(packet.RoomNumber);
        if (room == null)
        {
            Console.WriteLine($"[C_RoomEnterReqHandler] ErrorCode: {ErrorCode.NullRoom}");
            return ErrorCode.NullRoom;
        }

        ErrorCode result = room.AddRoomUser(user.Id, sessionId);
        if (result != ErrorCode.None)
        {
            Console.WriteLine($"[C_RoomEnterReqHandler] ErrorCode: {result}");
            return result;
        }

        _logger.Info($"[{packet.RoomNumber}번 room] Uid {sessionId} 입장, 현재 인원: {room.GetRoomUserCount()}");

        // 방 입장 성공 응답
        PKTResRoomEnter sendData = new PKTResRoomEnter();
        sendData.UserId = user.Id;
        sendData.RoomNumber = packet.RoomNumber;

        foreach(RoomUser ru in room.GetRoomUserList())
        {
            Console.WriteLine($"entered: {user.Id}, list: {ru.UserId}");
            sendData.RoomUserList.Add(ru.UserId);
        }

        room.NotifyRoomUsers(NetSendFunc, sendData, PACKETID.PKTResRoomEnter);

        // 서버 유저 상태 변경
        user.EnteredRoom(packet.RoomNumber);

        return ErrorCode.None;
    }


    public ErrorCode LeaveRoom(PKTReqRoomLeave packet, string sessionId)
    {
        (ErrorCode result, User? user, Room? room) = GetUserAndRoomBySessionId(sessionId);

        if (result != ErrorCode.None || user == null || room == null)
        {
            return ErrorCode.FailLeaveRoom;
        }


        // 룸에서 유저 삭제
        result = room.RemoveUserBySessionId(sessionId, _roomManager.GetPlayableRoomNumbers());
        if (result != ErrorCode.None)
        {
            Console.WriteLine($"[C_LeaveRoomReqHandler] ErrorCode: {result}");
            return result;
        }

        _logger.Info($"[{room.RoomNumber}번 room] Uid {sessionId} 퇴장, 현재 인원: {room.GetRoomUserCount()}");

        // 방 퇴장 성공 응답
        PKTResRoomLeave sendData = new PKTResRoomLeave();
        sendData.UserId = user.Id;
        room.NotifyRoomUsers(NetSendFunc, sendData, PACKETID.PKTResRoomLeave);

        // 방을 나간 유저에게는 따로 보낸다.
        SendPacket<PKTResRoomLeave>(sendData, PACKETID.PKTResRoomLeave, sessionId);

        // 서버 유저 상태 변경
        user.LeavedRoom();

        return ErrorCode.None;
    }


    public ErrorCode ChatRoom(PKTReqRoomChat packet, string sessionId) 
    {
        (ErrorCode result, User? user, Room? room) = GetUserAndRoomBySessionId(sessionId);

        if (result != ErrorCode.None || user == null || room == null)
        {
            return ErrorCode.FailLeaveRoom;
        }

        // 채팅 메시지 전달
        PKTResRoomChat sendData = new PKTResRoomChat();
        sendData.UserId = user.Id;
        sendData.ChatMsg = packet.ChatMsg;
        room.NotifyRoomUsers<PKTResRoomChat>(NetSendFunc, sendData, PACKETID.PKTResRoomChat);

        return ErrorCode.None;
    }

    public (ErrorCode, User?, Room?) GetUserAndRoomBySessionId(string sessionId)
    {
        User? user = _userManager.GetUserBySessionId(sessionId);
        if (user == null)
        {
            Console.WriteLine($"[C_LeaveRoomReqHandler] ErrorCode: {ErrorCode.NullUser}");
            return (ErrorCode.NullUser, null, null);
        }

        // 유저가 방에 입장한 상태인가
        if (user.RoomNumber == -1)
        {
            Console.WriteLine($"[C_LeaveRoomReqHandler] ErrorCode: {ErrorCode.InvalidRequest}");
            return (ErrorCode.InvalidRequest, null, null);
        }

        // 유저가 입장한 방이 존재하는가
        Room? room = _roomManager.FindRoomByRoomNumber(user.RoomNumber);
        if (room == null)
        {
            Console.WriteLine($"[C_LeaveRoomReqHandler] ErrorCode: {ErrorCode.NullRoom}");
            return (ErrorCode.NullRoom, null, null);
        }

        return (ErrorCode.None, user, room);
    }
}
