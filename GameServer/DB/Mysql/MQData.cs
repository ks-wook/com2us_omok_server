using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DB.Mysql;




[MemoryPackable]
public partial class MQReqSaveGameResult : PkHeader // 게임 결과 저장 요청
{
    public List<string> sessionIds = new List<string>();
    public string BlackUserId { get; set; } = string.Empty;
    public string WhiteUserId { get; set; } = string.Empty;
    public string WinUserId { get; set; } = string.Empty;

}

[MemoryPackable]
public partial class MQResSaveGameResult : PacketResult // 게임 결과 저장 요청 응답
{
    public List<string> sessionIds = new List<string>();
    public string WinUserId { get; set; } = string.Empty;

}

