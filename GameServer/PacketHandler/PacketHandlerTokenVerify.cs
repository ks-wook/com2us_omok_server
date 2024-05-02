using CloudStructures;
using CloudStructures.Structures;
using GameAPIServer.Model.DAO.MemoryDb;
using GameAPIServer.Repository;
using GameServer.Packet;
using MySqlConnector;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.PacketHandler;

public class PacketHandlerTokenVerify : BasePacketHandler
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
        packetHandlerMap.Add((int)InnerPacketId.PKTInnerReqVerifyToken, PKTInnerReqVerifyTokenHandler);
    }





    // redis에 저장된 토큰 검증
    public async Task PKTInnerReqVerifyTokenHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        (ErrorCode result, PKTInnerReqVerifyToken? bodyData) = DeserializePacket<PKTInnerReqVerifyToken>(packet.Data);

        if (result != ErrorCode.None || bodyData == null)
        {
            MainServer.MainLogger.Error("토큰 검증 실패");
            SendRedisFailPacket<PKTInnerResVerifyToken>(InnerPacketId.PKTInnerResVerifyToken, _packetProcessor, ErrorCode.LoginFail);
            return;
        }


        (result, LoginToken? loginToken) = await GetTokenByAccountId(bodyData.AccountId);
        if (result != ErrorCode.None || loginToken == null)
        {
            MainServer.MainLogger.Error("토큰 검증 실패");
            SendRedisFailPacket<PKTInnerResVerifyToken>(InnerPacketId.PKTInnerResVerifyToken, _packetProcessor, ErrorCode.LoginFail);
            return;
        }


        ResVerifyToken(bodyData.Token, loginToken.Token, sessionId, bodyData.AccountId.ToString());
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
    public void ResVerifyToken(string reqToken, string redisToken, string sessionId, string userId)
    {
        // 얻어온 토큰 검사
        if (string.CompareOrdinal(reqToken, redisToken) != 0) // 토큰이 일치하지 않는 경우
        {
            MainServer.MainLogger.Error("토큰 검증 실패");
            SendRedisFailPacket<PKTInnerResVerifyToken>(InnerPacketId.PKTInnerResVerifyToken, _packetProcessor, ErrorCode.TokenMismatch);
            return;
        }

        MainServer.MainLogger.Info("토큰 검증 성공");


        // 토큰 인증 완료 패킷 전송
        PKTInnerResVerifyToken sendData = new PKTInnerResVerifyToken();
        sendData.UserId = userId;
        sendData.Result = ErrorCode.None;
        SendRedisResPacket<PKTInnerResVerifyToken>(sendData, InnerPacketId.PKTInnerResVerifyToken, sessionId, _packetProcessor);
    }




}
