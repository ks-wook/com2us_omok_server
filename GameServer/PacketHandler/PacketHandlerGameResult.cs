using GameServer.Packet;
using MySqlConnector;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.PacketHandler;

public class PacketHandlerGameResult : BasePacketHandler
{
    MySqlConnection _dbConnector;
    SqlKata.Compilers.MySqlCompiler _dbCompiler;
    QueryFactory _queryFactory;

    PacketProcessor _packetProcessor;



    public PacketHandlerGameResult(MySqlConnection mysqlGameConnection, PacketProcessor packetProcessor)
    {
        _dbConnector = mysqlGameConnection;
        _dbCompiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new QueryFactory(_dbConnector, _dbCompiler);
        _packetProcessor = packetProcessor;
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
            MainServer.MainLogger.Error("게임 데이터 저장 실패");
            return;
        }

        result = await InsertGameResult(bodyData);
        if (result != ErrorCode.None)
        {
            SendMysqlFailPacket<PKTNtfEndOmok>(InnerPacketId.PKTInnerResSaveGameResult, _packetProcessor, ErrorCode.FailInsertGameResult);
            MainServer.MainLogger.Error("게임 데이터 저장 실패");
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
        SendInnerResPacket<PKTInnerResSaveGameResult>(sendData, InnerPacketId.PKTInnerResSaveGameResult, sessionId, _packetProcessor);
    }
}
