﻿using GameServer.Packet;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class PacketHandlerGame : BasePacketHandler
{
    MysqlProcessor _mysqlProcessor;

    RoomManager _roomManager;
    UserManager _userManager;

    public PacketHandlerGame(RoomManager? roomManager, UserManager? userManger, MysqlProcessor mysqlProcessor)
    {
        if (roomManager == null || userManger == null || mysqlProcessor == null)
        {
            MainServer.MainLogger.Error("[RoomPacketHandler.Init] Fail Create PacketHandlerGame");
            throw new NullReferenceException();
        }

        this._roomManager = roomManager;
        this._userManager = userManger;
        this._mysqlProcessor = mysqlProcessor;
    }


    public override void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap)
    {
        // Client req
        packetHandlerMap.Add((int)PACKETID.PKTReqReadyOmok, PKTReqReadyOmokHandler);
        packetHandlerMap.Add((int)PACKETID.PKTReqPutMok, PKTReqPutMokHandler);


        // DB res
        packetHandlerMap.Add((int)InnerPacketId.PKTInnerResSaveGameResult, MqResSaveGameResultHandler);

    }

    // 오목 준비 완료 요청
    public void PKTReqReadyOmokHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        (ErrorCode result, PKTReqReadyOmok? bodyData) = DeserializePacket<PKTReqReadyOmok>(packet.Data);
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

        (ErrorCode result, PKTReqPutMok? bodyData) = DeserializePacket<PKTReqPutMok>(packet.Data);
        if (result != ErrorCode.None || bodyData == null)
        {
            SendFailPacket<PKTResPutMok>(PACKETID.PKTResRoomChat, sessionId, result);
            return;
        }

        result = PutMok(bodyData, sessionId);
        if (result != ErrorCode.None)
        {
            SendFailPacket<PKTResPutMok>(PACKETID.PKTResPutMok, sessionId, result);
        }
    }


    // 게임 결과 DB 저장 완료 응답
    public void MqResSaveGameResultHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        (ErrorCode result, PKTInnerResSaveGameResult? bodyData) = DeserializePacket<PKTInnerResSaveGameResult>(packet.Data);
        if (result != ErrorCode.None || bodyData == null)
        {
            MainServer.MainLogger.Error("DB 패킷 수신 실패");
            return;
        }

        NotifyEndGame(bodyData);
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
            MainServer.MainLogger.Error($"[ReadyOmok] ErrorCode: {ErrorCode.NullUser}");
            return ErrorCode.NullUser;
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


        room.GetOmokGame().PlaceStone(packet.PosX, packet.PosY, user.Id);

        // 돌을 두었다는 완료 패킷 전송
        PKTResPutMok sendData = new PKTResPutMok();
        sendData.UserId = user.Id;
        sendData.PosX = packet.PosX;
        sendData.PosY = packet.PosY;
        room.NotifyRoomUsers<PKTResPutMok>(NetSendFunc, sendData, PACKETID.PKTResPutMok);


        // 승부가 났는지 확인
        if (room.GetOmokGame().CheckWinner(packet.PosX, packet.PosY))
        {
            // TODO 룸의 게임 타이머 종료









            // 종료된 게임을 db에 저장한다.
            PKTInnerReqSaveGameResult sendDBData = new PKTInnerReqSaveGameResult();
            foreach (RoomUser roomUser in room.GetRoomUserList())
            {
                sendDBData.sessionIds.Add(roomUser.RoomSessionID);
            }

            sendDBData.BlackUserId = room.GetOmokGame().blackUserId;
            sendDBData.WhiteUserId = room.GetOmokGame().whiteUserId;
            sendDBData.WinUserId = room.GetOmokGame().winUserId;

            // mysql processor로 전송
            SendMysqlReqPacket<PKTInnerReqSaveGameResult>(sendDBData, InnerPacketId.PKTInnerReqSaveGameResult, sessionId, _mysqlProcessor);

            return ErrorCode.None;
        }


        // TODO 룸의 게임 타이머 재시작










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


        room.NotifyRoomUsers<PKTNtfEndOmok>(NetSendFunc, endGameData, PACKETID.PKTNtfEndOmok);
    }




    public (ErrorCode, User?, Room?) GetUserAndRoomBySessionId(string sessionId)
    {
        User? user = _userManager.GetUserBySessionId(sessionId);
        if (user == null)
        {
            MainServer.MainLogger.Error($"[GetUserAndRoomBySessionId] ErrorCode: {ErrorCode.NullUser}");
            return (ErrorCode.NullUser, null, null);
        }

        // 유저가 방에 입장한 상태인가
        if (user.RoomNumber == -1)
        {
            MainServer.MainLogger.Error($"[GetUserAndRoomBySessionId] ErrorCode: {ErrorCode.InvalidRequest}");
            return (ErrorCode.InvalidRequest, null, null);
        }

        // 유저가 입장한 방이 존재하는가
        Room? room = _roomManager.FindRoomByRoomNumber(user.RoomNumber);
        if (room == null)
        {
            MainServer.MainLogger.Error($"[GetUserAndRoomBySessionId] ErrorCode: {ErrorCode.NullRoom}");
            return (ErrorCode.NullRoom, null, null);
        }

        return (ErrorCode.None, user, room);
    }

}
