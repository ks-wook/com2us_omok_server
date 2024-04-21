namespace GameAPIServer.Model.DAO.GameDb
{
    public class UserGameData
    {
        public Int64 uid { get; set; }
        public Int64 account_id {  get; set; }
        public string nickname { get; set; } = string.Empty;
        public int user_money { get; set; } = 0;
        public string created_at { get; set; } = string.Empty;
        public string recent_login_at { get; set; } = string.Empty;
        public int user_level { get; set; } = 0;
        public int user_exp { get; set; } = 0;
        public int total_win_cnt { get; set; } = 0;
        public int total_lose_cnt { get; set; } = 0;
    }
}
