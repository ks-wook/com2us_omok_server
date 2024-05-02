﻿using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DB.Redis;

[MemoryPackable]
public partial class MQReqVerifyToken : PkHeader // 로그인 토큰 검증 요청
{
    public Int64 AccountId { get; set; } // redis에 저장되는 토큰은 userId가 아닌 accountId를 key로 저장
    public string Token {  get; set; } = string.Empty;
}

[MemoryPackable]
public partial class MQResVerifyToken : PacketResult // 로그인 토큰 검증 요청 응답
{
    public string UserId { get; set; } = string.Empty;
}