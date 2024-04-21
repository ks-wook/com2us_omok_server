using GameAPIServer.Model.DAO.GameDb;

namespace GameAPIServer.Repository;

public interface IGameDb : IDisposable
{
    // 게임 데이터 생성 시 유저 계정에 묶인 accountId 가 있어야 한다.
    
    // UserGameData
    public Task<(ErrorCode, UserGameData?)> CreateUserGameData(Int64 accountId);
    public Task<(ErrorCode, UserGameData?)> GetUserGameDataByAccountId(Int64 accountId);
    public Task<(ErrorCode, UserGameData?)> GetGameDataByUid(Int64 uid);




    // Friend
    public Task<ErrorCode> CreateFriend(Int64 uid, Int64 friendUid);
    public Task<ErrorCode> DeleteFriend(Int64 uid, Int64 friendUid);
    public Task<ErrorCode> AcceptFriendReq(Int64 uid, Int64 friendUid);
    public Task<ErrorCode> RejectFriendReq(Int64 uid, Int64 friendUid);
    public Task<(ErrorCode, Friend?)> GetFriemdDataByUidAndFriendUid(Int64 uid, Int64 friendUid);
    public Task<(ErrorCode, IEnumerable<Friend?>?)> GetFriendListByUid(Int64 uid);



    // Mail
    public Task<(ErrorCode, Mail?)> GetMailByMailId(Int64 mailId);
    public Task<(ErrorCode, IEnumerable<Mail?>?)> GetMailListByUid(Int64 uid);
    public Task<ErrorCode> DeleteMailByMailid(Int64 mailId);
    public Task<ErrorCode> UpdateMailRewardedBymailId(Int64 mailId);



    // Mail Item
    public Task<(ErrorCode, IEnumerable<MailItem?>?)> GetMailItemListByMailId(Int64 mailId);
    public Task<ErrorCode> CreateMailItem(Int64 mailId, Int64 ItemTemplateId, int itemCount);


    // Item
    public Task<ErrorCode> CreateItem(Int64 uid, Int64 itemTemplateId, int itemCount);
    public Task<ErrorCode> CreateItemList(Int64 uid, IEnumerable<Item> itemList);
    public Task<ErrorCode> CreateItemListByMailItemList(Int64 uid, IEnumerable<MailItem?>? itemList);
    public Task<(ErrorCode, IEnumerable<Item>?)> GetItemListByUid(Int64 uid);

}
