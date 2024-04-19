namespace GameAPIServer.Model.DAO.GameDb
{
    public class Friend
    {
        public Int64 uid { get; set; }
        public Int64 friend_uid { get; set; }
        public bool friend_yn { get; set; } = false;
        public string created_at { get; set; } = string.Empty;
    }
}
