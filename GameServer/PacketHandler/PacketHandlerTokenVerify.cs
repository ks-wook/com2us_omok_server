﻿using CloudStructures;
using CloudStructures.Structures;
using GameAPIServer.Model.DAO.MemoryDb;
using GameAPIServer.Repository;
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

public class PacketHandlerTokenVerify : BasePacketHandler
{
    ILog _logger;

    RedisConnection _redisConnector;

    Action<MemoryPackBinaryRequestInfo> _mainPacketInsert;

    public PacketHandlerTokenVerify(ILog looger, RedisConnection redisConnector, Action<MemoryPackBinaryRequestInfo> mainPacketInsert)
    {
        _logger = looger;

        if(mainPacketInsert == null)
        {
            _logger.Error("mainPacket Insert null");
            throw new NullReferenceException();
        }

        _redisConnector = redisConnector;
        _mainPacketInsert = mainPacketInsert;
    }

    public override void RegisterPacketHandler(Dictionary<int, Func<MemoryPackBinaryRequestInfo, Task>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)InnerPacketId.PKTInnerReqVerifyToken, PKTInnerReqVerifyTokenHandler);
    }

    // redis에 저장된 토큰 검증
    public async Task PKTInnerReqVerifyTokenHandler(MemoryPackBinaryRequestInfo packet)
    {
        var sessionId = packet.SessionID;

        (ErrorCode result, PKTInnerReqVerifyToken? bodyData) = DeserializeNullablePacket<PKTInnerReqVerifyToken>(packet.Data);

        if (result != ErrorCode.None || bodyData == null)
        {
            _logger.Error("토큰 검증 실패");
            SendInnerFailPacket<PKTInnerResVerifyToken>(InnerPacketId.PKTInnerResVerifyToken, ErrorCode.LoginFail, _mainPacketInsert);
            return;
        }

        (result, LoginToken? loginToken) = await GetTokenByUid(bodyData.Uid);
        if (result != ErrorCode.None || loginToken == null)
        {
            _logger.Error("토큰 검증 실패");
            SendInnerFailPacket<PKTInnerResVerifyToken>(InnerPacketId.PKTInnerResVerifyToken, ErrorCode.LoginFail, _mainPacketInsert);
            return;
        }

        ResVerifyToken(bodyData.Token, loginToken.Token, sessionId, bodyData.Uid.ToString());
    }

    // 유저 uid 로 유효 토큰 검색
    public async Task<(ErrorCode, LoginToken?)> GetTokenByUid(long uid)
    {
        // uid를 이용해서 토큰을 검색한 후 검색된 반환한다.
        var key = MemoryDbKeyGenerator.GenLoginTokenKey(uid.ToString());

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
            _logger.Error("토큰 검증 실패");
            SendInnerFailPacket<PKTInnerResVerifyToken>(InnerPacketId.PKTInnerResVerifyToken, ErrorCode.TokenMismatch, _mainPacketInsert);
            return;
        }

        _logger.Info("토큰 검증 성공");

        // 토큰 인증 완료 패킷 전송
        PKTInnerResVerifyToken sendData = new PKTInnerResVerifyToken();
        sendData.UserId = userId;
        sendData.Result = ErrorCode.None;
        SendInnerResPacket<PKTInnerResVerifyToken>(sendData, InnerPacketId.PKTInnerResVerifyToken, sessionId, _mainPacketInsert);
    }

}
