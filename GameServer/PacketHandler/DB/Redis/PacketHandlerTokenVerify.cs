using CloudStructures;
using CloudStructures.Structures;
using GameAPIServer.Model.DAO.MemoryDb;
using GameAPIServer.Repository;
using GameServer.DB.Redis;
using GameServer.Packet;
using MySqlConnector;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class PacketHandlerTokenVerify : PacketHandler
{
    RoomManager _roomManager;
    UserManager _userManager;


    RedisConnection _redisConnector;

    PacketProcessor _packetProcessor;



    public PacketHandlerTokenVerify(RoomManager roomManager, UserManager userManager, RedisConnection redisConnector, PacketProcessor packetProcessor)
    {
        _roomManager = roomManager;
        _userManager = userManager;

        _redisConnector = redisConnector;
        _packetProcessor = packetProcessor;
    }


    public override void RegisterPacketHandler(Dictionary<int, Func<MemoryPackBinaryRequestInfo, Task>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)MQDATAID.MQ_REQ_VERIFY_TOKEN, MqReqVerifyTokenHandler);
    }





    // redis에 저장된 토큰 검증
    public async Task MqReqVerifyTokenHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        (ErrorCode result, MQReqVerifyToken? bodyData) = DeserializePacket<MQReqVerifyToken>(packet.Data);

        if (result != ErrorCode.None || bodyData == null)
        {
            Console.WriteLine("토큰 검증 실패");
            SendRedisFailPacket<MQResVerifyToken>(MQDATAID.MQ_RES_VERIFY_TOKEN, _packetProcessor, result);
            return;
        }


        (result, LoginToken? loginToken) = await GetTokenByAccountId(bodyData.AccountId);
        if(result != ErrorCode.None || loginToken == null)
        {
            Console.WriteLine("토큰 검증 실패");
            SendRedisFailPacket<MQResVerifyToken>(MQDATAID.MQ_RES_VERIFY_TOKEN, _packetProcessor, result);
            return;
        }


        ResVerifyToken(bodyData.Token, loginToken.Token, sessionId);
    }





    // 유저 accountId로 유효 토큰 검색
    public async Task<(ErrorCode, LoginToken?)> GetTokenByAccountId(long accountId)
    {
        // accountId를 이용해서 토큰을 검색한 후 검색된 반환한다.
        var key = MemoryDbKeyGenerator.GenLoginTokenKey(accountId.ToString());

        try
        {
            RedisString<LoginToken> redis = new(_redisConnector, key, null);
            RedisResult<LoginToken> loginToken = await redis.GetAsync();
            if (!loginToken.HasValue)
            {
                return (ErrorCode.NullLoginToken, null);
            }

            return (ErrorCode.None, loginToken.Value);
        }
        catch
        {
            return (ErrorCode.NullLoginToken, null);
        }

    }

    // 토큰 검증 후 요청 응답
    public void ResVerifyToken(string reqToken, string redisToken, string sessionId)
    {

        Console.WriteLine("reqToken : " + reqToken);
        Console.WriteLine("redisToken : " + redisToken);


        // 얻어온 토큰 검사
        if (string.CompareOrdinal(reqToken, redisToken) != 0) // 토큰이 일치하지 않는 경우
        {
            Console.WriteLine("토큰 검증 실패");
            SendRedisFailPacket<MQResVerifyToken>(MQDATAID.MQ_RES_VERIFY_TOKEN, _packetProcessor, ErrorCode.TokenMismatch);
            return;
        }


        // 토큰 인증 완료 패킷 전송
        MQResVerifyToken sendData = new MQResVerifyToken();
        sendData.Result = ErrorCode.None;
        SendRedisResPacket<MQResVerifyToken>(sendData, MQDATAID.MQ_RES_VERIFY_TOKEN, sessionId, _packetProcessor);
    }




}
