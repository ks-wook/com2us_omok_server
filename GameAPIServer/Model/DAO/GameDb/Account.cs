﻿namespace GameAPIServer.Model.DAO.GameDb
{
    // account 테이블 아이템 접근 객체
    public class Account
    {
        public long account_id { get; set; }
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string saltValue { get; set; } = string.Empty;
        public string created_at { get; set; } = string.Empty;
        public string recent_login_at { get; set; } = string.Empty;
    }
}
