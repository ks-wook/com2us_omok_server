namespace GameAPIServer.Model.DAO.GameDb
{
    public class MailItem
    {
        public Int64 mail_item_id { get; set; }
        public Int64 mail_id { get; set; }
        public Int64 item_template_id { get; set; }
        public int item_count { get; set; }
        public bool receive_yn { get; set; } = false;
    }
}
