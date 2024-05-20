using GameAPIServer.Model.DAO.GameDb;
using GameServer.Packet;
using MySqlConnector;
using PacketData;
using SqlKata.Execution;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.PacketHandler;

public class PacketHandlerGameResult : BasePacketHandler
{
    ILog _logger;
    Action<MemoryPackBinaryRequestInfo> _mainPacketInsert;

    public PacketHandlerGameResult(ILog logger, Action<MemoryPackBinaryRequestInfo> mainPacketInsert)
    {
        _logger = logger;
        _mainPacketInsert = mainPacketInsert;
    }

    public override void RegisterPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo, QueryFactory>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)InnerPacketId.PKTInnerReqSaveGameResult, PKTInnerReqSaveGameResultHandler);
    }

    // 게임 결과 저장
    public void PKTInnerReqSaveGameResultHandler(MemoryPackBinaryRequestInfo packet, QueryFactory queryFactory)
    {
        var sessionId = packet.SessionID;

        (ErrorCode result, PKTInnerReqSaveGameResult? bodyData) = DeserializeNullablePacket<PKTInnerReqSaveGameResult>(packet.Data);

        if (result != ErrorCode.None || bodyData == null)
        {
            _logger.Error("게임 데이터 저장 실패");
            return;
        }

        result = InsertGameResult(bodyData, queryFactory);
        if (result != ErrorCode.None)
        {
            SendInnerFailPacket<PKTNtfEndOmok>(InnerPacketId.PKTInnerResSaveGameResult, ErrorCode.FailInsertGameResult, _mainPacketInsert);
            _logger.Error("[PKTInnerReqSaveGameResultHandler] 게임 데이터 저장 실패");
            return;
        }

        result = UpdateUserGameRecord(bodyData, queryFactory);
        if (result != ErrorCode.None)
        {
            SendInnerFailPacket<PKTNtfEndOmok>(InnerPacketId.PKTInnerResSaveGameResult, ErrorCode.NullUserGameData, _mainPacketInsert);
            _logger.Error("[PKTInnerReqSaveGameResultHandler] 유저 전적 업데이트 실패");
            return;
        }

        ResSaveGameResult(bodyData, sessionId);
    }

    public ErrorCode InsertGameResult(PKTInnerReqSaveGameResult packet, QueryFactory queryFactory)
    {
        // string -> Int62 parsing
        long blackUserId = long.Parse(packet.BlackUserId);
        long whiteUserId = long.Parse(packet.WhiteUserId);
        long winUserId = long.Parse(packet.WinUserId);

        // 게임 결과 데이터 저장
        var insertSuccess = queryFactory.Query("game_result")
                .Insert(new
                {
                    black_user_id = blackUserId,
                    white_user_id = whiteUserId,
                    win_user_id = winUserId,
                });

        // 저장 실패
        if (insertSuccess != 1)
        {
            return ErrorCode.FailInsertGameResult;
        }

        _logger.Info("게임 데이터 저장 성공");

        return ErrorCode.None;
    }

    // 유저 전적 업데이트
    public ErrorCode UpdateUserGameRecord(PKTInnerReqSaveGameResult packet, QueryFactory queryFactory)
    {
        string loseUserId = "";

        if (packet.BlackUserId == packet.WinUserId)
        {
            loseUserId = packet.WhiteUserId;
        }
        else
        {
            loseUserId = packet.BlackUserId;
        }

        // 승자 전적
        var winUserGameData = queryFactory.Query("user_game_data").Where("account_id", packet.WinUserId).FirstOrDefault<UserGameData>();

        if(winUserGameData == null)
        {
            _logger.Error($"[UpdateUserGameRecord] ErrorCode: {ErrorCode.NullUserGameData}");
            return ErrorCode.NullUserGameData;
        }

        queryFactory.Query("user_game_data").Where("account_id", packet.WinUserId).Update(new
        {
            total_win_cnt = winUserGameData.total_win_cnt + 1
        });

        // 패자 전적
        var loseUserGameData = queryFactory.Query("user_game_data").Where("account_id", loseUserId).FirstOrDefault<UserGameData>();

        if (loseUserGameData == null)
        {
            _logger.Error($"[UpdateUserGameRecord] ErrorCode: {ErrorCode.NullUserGameData}");
            return ErrorCode.NullUserGameData;
        }

        queryFactory.Query("user_game_data").Where("account_id", loseUserId).Update(new
        {
            total_lose_cnt = loseUserGameData.total_lose_cnt + 1
        });

        return ErrorCode.None;
    }

    public void ResSaveGameResult(PKTInnerReqSaveGameResult packet, string sessionId)
    {
        // 게임 결과 저장 완료 패킷 전송
        PKTInnerResSaveGameResult sendData = new PKTInnerResSaveGameResult();
        sendData.sessionIds = packet.sessionIds;
        sendData.WinUserId = packet.WinUserId;
        SendInnerResPacket<PKTInnerResSaveGameResult>(sendData, InnerPacketId.PKTInnerResSaveGameResult, sessionId, _mainPacketInsert);
    }
}