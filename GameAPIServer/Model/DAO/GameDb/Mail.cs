namespace GameAPIServer.Model.DAO.GameDb
{
    public class Mail
    {
        public Int64 mail_id {  get; set; }
        public Int64 uid { get; set; }
        public Int64 mail_template_id { get; set; }
        public bool receive_yn { get; set; }
    }
}
