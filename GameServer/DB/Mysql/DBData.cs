using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DB.Mysql;

// game_result 테이블 접근 객체
public class GameResult
{
    public long game_result_id { get; set; }
    public long black_user_id { get; set; }
    public long white_user_id { get; set; }
    public long win_user_id { get; set; }
    public string created_at { get; set; } = string.Empty;
}
