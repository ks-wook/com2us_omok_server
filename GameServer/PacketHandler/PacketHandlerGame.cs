﻿using GameServer.Packet;
using MemoryPack;
using PacketData;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class PacketHandlerGame : BasePacketHandler
{
    ILog _logger;

    Action<MemoryPackBinaryRequestInfo> _mysqlInsert;

    RoomManager _roomManager;
    UserManager _userManager;

    public PacketHandlerGame(ILog logger, RoomManager? roomManager, UserManager? userManger, Action<MemoryPackBinaryRequestInfo> mysqlInsert)
    {
        _logger = logger;

        if (roomManager == null || userManger == null || mysqlInsert == null)
        {
            _logger.Error("[RoomPacketHandler.Init] Fail Create PacketHandlerGame");
            throw new NullReferenceException();
        }

        _roomManager = roomManager;
        _userManager = userManger;
        _mysqlInsert = mysqlInsert;
    }

    public override void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap)
    {
        // Client req
        packetHandlerMap.Add((int)PACKETID.PKTReqReadyOmok, PKTReqReadyOmokHandler);
        packetHandlerMap.Add((int)PACKETID.PKTReqPutMok, PKTReqPutMokHandler);

        // InnerPacket
        packetHandlerMap.Add((int)InnerPacketId.PKTInnerResSaveGameResult, PKTInnerResSaveGameResultHandler);
        packetHandlerMap.Add((int)InnerPacketId.PKTInnerNtfTurnChange, PKTInnerNtfTurnChangeHandler);
    }

    // 오목 준비 완료 요청
    public void PKTReqReadyOmokHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        (ErrorCode result, PKTReqReadyOmok? bodyData) = DeserializeNullablePacket<PKTReqReadyOmok>(packet.Data);
        if (result != ErrorCode.None || bodyData == null)
        {
            SendFailPacket<PKTResReadyOmok>(PACKETID.PKTResReadyOmok, sessionId, result);
            return;
        }

        result = ReadyOmok(bodyData, sessionId);
        if (result != ErrorCode.None)
        {
            SendFailPacket<PKTResReadyOmok>(PACKETID.PKTResReadyOmok, sessionId, result);
        }
    }

    // 오목 돌을 두는 요청
    public void PKTReqPutMokHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        (ErrorCode result, PKTReqPutMok? bodyData) = DeserializeNullablePacket<PKTReqPutMok>(packet.Data);
        if (result != ErrorCode.None || bodyData == null)
        {
            SendFailPacket<PKTResPutMok>(PACKETID.PKTResPutMok, sessionId, result);
            return;
        }

        result = PutMok(bodyData, sessionId);
        if (result != ErrorCode.None)
        {
            SendFailPacket<PKTResPutMok>(PACKETID.PKTResPutMok, sessionId, result);
        }
    }

    // 게임 결과 DB 저장 완료 응답
    public void PKTInnerResSaveGameResultHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        PKTInnerResSaveGameResult? bodyData = DeserializePacket<PKTInnerResSaveGameResult>(packet.Body);

        NotifyEndGame(bodyData);
    }

    // 강제로 턴을 바꾸는 패킷 핸들러
    public void PKTInnerNtfTurnChangeHandler(MemoryPackBinaryRequestInfo packet)
    {
        var bodyData = DeserializePacket<PKTInnerNtfTurnChange>(packet.Data);

        ForceTurnChange(bodyData);
    }

    public ErrorCode ReadyOmok(PKTReqReadyOmok packet, string sessionId)
    {
        (ErrorCode result, User? user, Room? room) = GetUserAndRoomBySessionId(sessionId);

        if (result != ErrorCode.None || user == null || room == null)
        {
            return ErrorCode.FailReadyOmok;
        }

        int readyResult = room.ChangeIsReadyBySessionId(sessionId);
        if (readyResult == -1)
        {
            _logger.Error($"[ReadyOmok] ErrorCode: {ErrorCode.NullUser}");
            return ErrorCode.NullUser;
        }
        else if( readyResult == 0)
        {
            _logger.Info($"[{sessionId}] 준비완료 취소");
        }
        else
        {
            _logger.Info($"[{sessionId}] 준비완료");
        }

        // 준비상태 변경 응답
        PKTResReadyOmok sendData = new PKTResReadyOmok();
        sendData.UserId = user.Id;

        if (readyResult == 0)
        {
            sendData.IsReady = false;

        }
        else if (readyResult == 1)
        {
            sendData.IsReady = true;
        }

        room.NotifyRoomUsers<PKTResReadyOmok>(NetSendFunc, sendData, PACKETID.PKTResReadyOmok);

        // 모든 유저의 준비가 끝났는지 검사
        if (room.CheckAllUsersReady())
        {
            room.OmokGameStart(NetSendFunc);
            _logger.Info($"RoomNumber: {room.RoomNumber}, 흑돌: {room.GetOmokGame().BlackUserId}, 백돌: {room.GetOmokGame().WhiteUserId} 게임 시작");
# pragma warning disable 8600, 8602
            foreach(RoomUser roomUser in room.GetRoomUserList())
            {
                User gameStartUser = _userManager.GetUserBySessionId(roomUser.RoomSessionID);
                gameStartUser.State = UserState.InGame;
            }
#pragma warning restore 8600, 8602
        }

        return ErrorCode.None;
    }

    public ErrorCode PutMok(PKTReqPutMok packet, string sessionId)
    {
        (ErrorCode result, User? user, Room? room) = GetUserAndRoomBySessionId(sessionId);

        if (result != ErrorCode.None || user == null || room == null)
        {
            return ErrorCode.FailReadyOmok;
        }

        if (room.CheckCurrentTurnUserId(user.Id) == false)
        {
            return ErrorCode.InvalidRequest;
        }

        room.GetOmokGame().PlaceStone(packet.PosX, packet.PosY, user.Id);

        // 돌을 두었다는 완료 패킷 전송
        PKTResPutMok sendData = new PKTResPutMok();
        sendData.UserId = user.Id;
        sendData.PosX = packet.PosX;
        sendData.PosY = packet.PosY;
        if(room.GetOmokGame().BlackUserId == user.Id)
        {
            sendData.IsBlack = true;
        }

        room.NotifyRoomUsers<PKTResPutMok>(NetSendFunc, sendData, PACKETID.PKTResPutMok);

        room.UpdateLastTurnChangeTime();
        room.GameCancelStack = 0;

        // 승부가 났는지 확인
        if (room.GetOmokGame().CheckWinner(packet.PosX, packet.PosY))
        {
            // 종료된 게임을 db에 저장한다.
            PKTInnerReqSaveGameResult sendDBData = new PKTInnerReqSaveGameResult();
            foreach (RoomUser roomUser in room.GetRoomUserList())
            {
                sendDBData.sessionIds.Add(roomUser.RoomSessionID);
            }

            sendDBData.BlackUserId = room.GetOmokGame().BlackUserId;
            sendDBData.WhiteUserId = room.GetOmokGame().WhiteUserId;
            sendDBData.WinUserId = room.GetOmokGame().WinUserId;

            // mysql processor로 전송
            SendInnerReqPacket<PKTInnerReqSaveGameResult>(sendDBData, InnerPacketId.PKTInnerReqSaveGameResult, sessionId, _mysqlInsert);

            _logger.Info($"게임 종료 승자 : uid: {room.GetOmokGame().WinUserId}");

            return ErrorCode.None;
        }

        return ErrorCode.None;
    }

    // 게임 종료 통보 전송
    public void NotifyEndGame(PKTInnerResSaveGameResult packet)
    {
        PKTNtfEndOmok endGameData = new PKTNtfEndOmok();
        endGameData.WinUserId = packet.WinUserId;

        User? user = _userManager.GetUserByUID(packet.WinUserId);
        if (user == null)
        {
            SendFailPacket<PKTNtfEndOmok>(PACKETID.PKTNtfEndOmok, packet.sessionIds, ErrorCode.NullUser);
            return;
        }

        Room? room = _roomManager.FindRoomByRoomNumber(user.RoomNumber);
        if (room == null)
        {
            SendFailPacket<PKTNtfEndOmok>(PACKETID.PKTNtfEndOmok, packet.sessionIds, ErrorCode.NullRoom);
            return;
        }

        room.OmokGameEnd();
        room.NotifyRoomUsers<PKTNtfEndOmok>(NetSendFunc, endGameData, PACKETID.PKTNtfEndOmok);

        // 유저 상태 변경
        foreach(RoomUser roomUser in room.GetRoomUserList())
        {
            user = _userManager.GetUserBySessionId(roomUser.RoomSessionID);

            if (user == null)
            {
                SendFailPacket<PKTNtfEndOmok>(PACKETID.PKTNtfEndOmok, packet.sessionIds, ErrorCode.NullUser);
                return;
            }

            user.State = UserState.InRoom;
        }
    }

    // 게임 취소 패킷 전송
    public void NotifyCancelGame(Room room)
    {
        PKTNtfEndOmok endGameData = new PKTNtfEndOmok();
        endGameData.WinUserId = "Game Cancel";

        room.OmokGameEnd();
        room.NotifyRoomUsers<PKTNtfEndOmok>(NetSendFunc, endGameData, PACKETID.PKTNtfEndOmok);

        // 유저 상태 변경
        foreach (RoomUser roomUser in room.GetRoomUserList())
        {
            User? user = _userManager.GetUserBySessionId(roomUser.RoomSessionID);

            if (user == null)
            {
                SendFailPacket<PKTNtfEndOmok>(PACKETID.PKTNtfEndOmok, roomUser.RoomSessionID, ErrorCode.NullUser);
                return;
            }

            user.State = UserState.InRoom;
        }
    }

    public (ErrorCode, User?, Room?) GetUserAndRoomBySessionId(string sessionId)
    {
        User? user = _userManager.GetUserBySessionId(sessionId);
        if (user == null)
        {
            _logger.Error($"[GetUserAndRoomBySessionId] ErrorCode: {ErrorCode.NullUser}");
            return (ErrorCode.NullUser, null, null);
        }

        // 유저가 방에 입장한 상태인가
        if (user.RoomNumber == -1)
        {
            _logger.Error($"[GetUserAndRoomBySessionId] ErrorCode: {ErrorCode.InvalidRequest}");
            return (ErrorCode.InvalidRequest, null, null);
        }

        // 유저가 입장한 방이 존재하는가
        Room? room = _roomManager.FindRoomByRoomNumber(user.RoomNumber);
        if (room == null)
        {
            _logger.Error($"[GetUserAndRoomBySessionId] ErrorCode: {ErrorCode.NullRoom}");
            return (ErrorCode.NullRoom, null, null);
        }

        return (ErrorCode.None, user, room);
    }

    // Inner Packet이므로 패킷이 깨지거나 유효하지않은 요청일수 없다.
    public void ForceTurnChange(PKTInnerNtfTurnChange bodyData)
    {
#pragma warning disable 8600, 8602
        Room room = _roomManager.FindRoomByRoomNumber(bodyData.RoomNumber);

        // 강제 턴 넘기기 + putMok 패킷이 절묘하게 겹치는 경우 방지
        if (room.CheckCurrentTurnUserId(bodyData.CurTurnUserId) == false)
        {
            return;
        }

        room.GameCancelStack++;

        bool gameCancel = room.CheckGameCancel();
        if(gameCancel) // 게임 취소
        {
            NotifyCancelGame(room);
            return;
        }

        // 턴 변경 패킷 전송
        PKTResPutMok sendData = new PKTResPutMok();
        sendData.UserId = room.CurTurnUserId;
        sendData.IsTimeout = true;
        room.NotifyRoomUsers<PKTResPutMok>(NetSendFunc, sendData, PACKETID.PKTResPutMok);
        
        // 턴 변경시간 업데이트
        room.UpdateLastTurnChangeTime();

#pragma warning restore 8600, 8602
    }
}
