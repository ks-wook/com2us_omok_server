using GameServer.DB.Mysql;
using GameServer.Packet;
using MySqlConnector;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class PacketHandlerGameResult : PacketHandler
{
    RoomManager _roomManager;
    UserManager _userManager;


    MySqlConnection _dbConnector;
    SqlKata.Compilers.MySqlCompiler _dbCompiler;
    QueryFactory _queryFactory;

    PacketProcessor _packetProcessor;



    public PacketHandlerGameResult(RoomManager roomManager, UserManager userManager, MySqlConnection mysqlGameConnection, PacketProcessor packetProcessor)
    {
        _roomManager = roomManager;
        _userManager = userManager;

        _dbConnector = mysqlGameConnection;
        _dbCompiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConnector, _dbCompiler);
        _packetProcessor = packetProcessor;
    }


    public override void RegisterPacketHandler(Dictionary<int, Func<byte[], Task>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)MQDATAID.MQ_REQ_SQVE_GAME_RESULT, MqReqGameRecordHandler);
    }





    // 게임 결과 저장
    public async Task MqReqGameRecordHandler(byte[] packet)
    {
        (ErrorCode result, MQReqSaveGameResult? bodyData) = DeserializePacket<MQReqSaveGameResult>(packet);

        if(result != ErrorCode.None || bodyData == null)
        {
            Console.WriteLine("게임 데이터 저장 실패");
            return;
        }

        result = await InsertGameResult(bodyData);
        if(result != ErrorCode.None) 
        {
            SendDBFailPacket<PKTNtfEndOmok>(MQDATAID.MQ_RES_SAVE_GAME_RESULT, _packetProcessor, ErrorCode.FailInsertGameResult);
            Console.WriteLine("게임 데이터 저장 실패");
            return;
        }

        ResSaveGameResult(bodyData);
    }





    public async Task<ErrorCode> InsertGameResult(MQReqSaveGameResult packet)
    {
        // string -> Int62 parsing
        Int64 blackUserId = Int64.Parse(packet.BlackUserId);
        Int64 whiteUserId = Int64.Parse(packet.WhiteUserId);
        Int64 winUserId = Int64.Parse(packet.WinUserId);

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

    
    public void ResSaveGameResult(MQReqSaveGameResult packet)
    {
        User? user = _userManager.GetUserByUID(packet.WinUserId);
        if (user == null)
        {
            SendDBFailPacket<MQResSaveGameResult>(MQDATAID.MQ_RES_SAVE_GAME_RESULT, _packetProcessor, ErrorCode.NullUser);
            return;
        }

        // 게임 결과 저장 완료 패킷 전송
        MQResSaveGameResult sendData = new MQResSaveGameResult();
        sendData.sessionIds = packet.sessionIds;
        sendData.WinUserId = packet.WinUserId;
        SendDBPacket<MQResSaveGameResult>(sendData, MQDATAID.MQ_RES_SAVE_GAME_RESULT, user.SessionId, _packetProcessor);
    }
}
