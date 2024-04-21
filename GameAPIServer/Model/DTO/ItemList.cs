using GameAPIServer.Model.DAO.GameDb;

namespace GameAPIServer.Model.DTO
{
    public class ItemListReq
    {
        public Int64 Uid { get; set; }
    }

    public class ItemListRes
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
        public IEnumerable<Item>? ItemList { get; set; }
    }

}
