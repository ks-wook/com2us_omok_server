namespace HiveServer.Model.DAO.HiveDb
{
    // account 테이블 아이템 접근 객체
    public class Account
    {
        public Int64 account_id { get; set; }
        public string id { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string saltValue { get; set; } = string.Empty;
        public string created_at { get; set; } = string.Empty;
        public string recent_login_at { get; set; } = string.Empty;
    }
}
