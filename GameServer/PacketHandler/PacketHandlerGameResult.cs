using GameServer.Packet;
using MySqlConnector;
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

    MySqlConnection _dbConnector;
    SqlKata.Compilers.MySqlCompiler _dbCompiler;
    QueryFactory _queryFactory;

    Action<MemoryPackBinaryRequestInfo> _mainPacketInsert;

    public PacketHandlerGameResult(ILog logger, MySqlConnection mysqlGameConnection, Action<MemoryPackBinaryRequestInfo> mainPacketInsert)
    {
        _logger = logger;

        _dbConnector = mysqlGameConnection;
        _dbCompiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new QueryFactory(_dbConnector, _dbCompiler);
        _mainPacketInsert = mainPacketInsert;
    }

    public override void RegisterPacketHandler(Dictionary<int, Func<MemoryPackBinaryRequestInfo, Task>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)InnerPacketId.PKTInnerReqSaveGameResult, MqReqGameRecordHandler);
    }

    // 게임 결과 저장
    public async Task MqReqGameRecordHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        (ErrorCode result, PKTInnerReqSaveGameResult? bodyData) = DeserializeNullablePacket<PKTInnerReqSaveGameResult>(packet.Data);

        if (result != ErrorCode.None || bodyData == null)
        {
            _logger.Error("게임 데이터 저장 실패");
            return;
        }

        result = await InsertGameResult(bodyData);
        if (result != ErrorCode.None)
        {
            SendInnerFailPacket<PKTNtfEndOmok>(InnerPacketId.PKTInnerResSaveGameResult, ErrorCode.FailInsertGameResult, _mainPacketInsert);
            _logger.Error("게임 데이터 저장 실패");
            return;
        }

        ResSaveGameResult(bodyData, sessionId);
    }

    public async Task<ErrorCode> InsertGameResult(PKTInnerReqSaveGameResult packet)
    {
        // string -> Int62 parsing
        long blackUserId = long.Parse(packet.BlackUserId);
        long whiteUserId = long.Parse(packet.WhiteUserId);
        long winUserId = long.Parse(packet.WinUserId);

        // 게임 결과 데이터 저장
        var insertSuccess = await _queryFactory.Query("game_result")
                .InsertAsync(new
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