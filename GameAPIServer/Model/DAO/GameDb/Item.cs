namespace GameAPIServer.Model.DAO.GameDb
{
    public class Item
    {
        public Int64 item_id {  get; set; }
        public Int64 item_template_id { get; set; }
        public Int64 owner_id { get; set; }
        public int item_count {  get; set; }
    }
}
