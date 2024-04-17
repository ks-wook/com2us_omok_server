namespace GameAPIServer.Model.DAO.GameDb
{
    public class User
    {
        public Int64 user_id { get; set; }
        public Int64 account_id {  get; set; }
        public string nickname { get; set; } = string.Empty;
        public int user_money { get; set; }
        public string created_at { get; set; } = string.Empty;
        public string recent_login_at { get; set; } = string.Empty;
        public int user_level { get; set; }
        public int user_exp { get; set; }
        public int total_win_cnt { get; set; }
        public int total_lose_cnt { get; set; }
    }
}
