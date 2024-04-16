namespace HiveServer.Model.DAO
{
    // account 테이블 아이템 접근 객체
    public class Account
    {
        public Int64 account_id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string created_at { get; set; }
        public string recent_login_at { get; set; }
    }
}
