using GameAPIServer.Model.DAO.GameDb;
using Microsoft.Extensions.Options;
using System.Data;
using MySqlConnector;
using SqlKata;
using ZLogger;
using SqlKata.Execution;

namespace GameAPIServer.Repository;

public class GameDb : IGameDb
{
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<GameDb> _logger;

    IDbConnection? _dbConnector;
    SqlKata.Compilers.MySqlCompiler _dbCompiler;
    QueryFactory _queryFactory;

    public static string gameDBConnectionStr = string.Empty;

    public GameDb(ILogger<GameDb> logger, IOptions<DbConfig> dbConfig)
    {
        _dbConfig = dbConfig;
        _logger = logger;

        DbConnect();

        _dbCompiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConnector, _dbCompiler);
    }


    // create connection with db
    private ErrorCode DbConnect()
    {
        gameDBConnectionStr = _dbConfig.Value.GameDb;
        if (gameDBConnectionStr == null) // db 연결문자열 가져오기 실패
        {
            _logger.ZLogError(
                $"[DbConnect] Null Db Connection String");

            return ErrorCode.NullAccountDbConnectionStr;
        }

        _dbConnector = new MySqlConnection(gameDBConnectionStr);
        _dbConnector.Open();

        return ErrorCode.None;
    }



    // disconnect with db
    private void DbDisconnect()
    {
        if(_dbConnector == null) 
        {
            _logger.ZLogError
                ($"[ErrorCode]: {ErrorCode.FailDisconnectGameDb}");
            return;
        }
        _dbConnector.Close();
    }

    public void Dispose()
    {
        DbDisconnect();
    }



    public async Task<(ErrorCode, UserGameData?)> CreateUserGameData(Int64 accountId)
    {
        try
        {
            var insertSuccess = await _queryFactory.Query("user_game_data")
                .InsertAsync(new
                {
                    account_id = accountId,
                    nickname = "User" + accountId,
                });

            _logger.ZLogDebug
                ($"[CreateUserGameData] accountId: {accountId}");


            if (insertSuccess != 1)
            {
                _logger.ZLogError
                    ($"[CreateUserGameData] ErrorCode: {ErrorCode.FailCreateNewGameData}, accountId: {accountId}");
                return (ErrorCode.FailCreateNewGameData, null);
            }


            // 데이터 삽입 성공, 삽입된 게임데이터 획득
            (ErrorCode result, UserGameData? userGameData) = await GetUserGameDataByAccountId(accountId);

            return (result, userGameData);
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.ToString());
            _logger.ZLogError
                    ($"[CreateUserGameData] ErrorCode: {ErrorCode.FailCreateNewGameData}, accountId: {accountId}");
            return (ErrorCode.FailCreateNewGameData, null);
        }
    }

    // AccountId를 이용해서 UserGameData Search
    public async Task<(ErrorCode, UserGameData?)> GetUserGameDataByAccountId(Int64 accountId)
    {
        try
        {
            UserGameData? data = await _queryFactory.Query("user_game_data")
            .Where("account_id", accountId).FirstOrDefaultAsync<UserGameData>();

            if (data == null) // 게임데이터가 존재하지 않는 경우
            {
                return (ErrorCode.NullUserGameData, null);
            }

            return (ErrorCode.None, data);
        }
        catch 
        {
            _logger.ZLogError
                ($"[GetUserGameDataByAccountId] ErrorCode: {ErrorCode.FailGetUserGameData}, accountId: {accountId}");
            return (ErrorCode.FailGetUserGameData, null);
        }
    }


    public async Task<(ErrorCode, UserGameData?)> GetGameDataByUid(Int64 uid)
    {
        try
        {
            UserGameData? data = await _queryFactory.Query("user_game_data")
            .Where("uid", uid).FirstOrDefaultAsync<UserGameData>();

            if (data == null) // 게임데이터가 존재하지 않는 경우
            {
                return (ErrorCode.NullUserGameData, null);
            }

            return (ErrorCode.None, data);
        }
        catch 
        {
            _logger.ZLogError
                ($"[CreateUserGameData] ErrorCode: {ErrorCode.NullUserGameData}, uid: {uid}");
            return (ErrorCode.NullUserGameData, null);
        }
    }


    public async Task<ErrorCode> UpdateGameDataRecentLoginByUid(Int64 uid)
    {
        try
        {
            int updateSuccess = await _queryFactory.Query("user_game_data")
                .Where("uid", uid).UpdateAsync(new { recent_login_at = DateTime.Now });
            
            if(updateSuccess != 1)
            {
                _logger.ZLogError
                    ($"[UpdateGameDataRecentLoginByUid] ErrorCode: {ErrorCode.FailUpdateRecentLogin}, uid: {uid}");
                return ErrorCode.FailUpdateRecentLogin;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError
                ($"[UpdateGameDataRecentLoginByUid] ErrorCode: {ErrorCode.FailUpdateRecentLogin}, uid: {uid}");
            return ErrorCode.FailUpdateRecentLogin;
        }
    }



    public async Task<ErrorCode> CreateFriend(Int64 uid, Int64 friendUid)
    {
        Friend? friend = new Friend()
        {
              uid = uid,
              friend_uid = friendUid
        };

        try
        {
            var insertSuccess = await _queryFactory.Query("friend")
                .InsertAsync(friend);

            _logger.ZLogDebug
                ($"[CreateFriend] uid: {uid}, friendUid: {friendUid}");


            if (insertSuccess != 1)
            {
                _logger.ZLogError
                    ($"[CreateFriend] uid: {uid}, friendUid: {friendUid}");
                return ErrorCode.FailCreateNewFriendData;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError
                    ($"[CreateFriend] uid: {uid}, friendUid: {friendUid}");
            return ErrorCode.FailCreateNewFriendData;
        }

        
    }

    public async Task<ErrorCode> DeleteFriend(Int64 uid, Int64 friendUid)
    {
        try
        {
            var delSuccess = await _queryFactory.Query("friend")
            .Where(q => q
                .Where("uid", uid)
                .Where("friend_uid", friendUid)
            )
            .OrWhere(q => q
                .Where("uid", friendUid)
                .Where("friend_uid", uid)
            )
            .DeleteAsync();

            if (delSuccess != 1)
            {
                _logger.ZLogError
                    ($"[DeleteFriend] ErrorCode: {ErrorCode.FailDeleteFriend}, uid: {uid}, friendUid{friendUid}");
                return ErrorCode.FailDeleteFriend;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError
                    ($"[DeleteFriend] ErrorCode: {ErrorCode.FailDeleteFriend}, uid: {uid}, friendUid{friendUid}");
            return ErrorCode.FailDeleteFriend;
        }

        
    }

    public async Task<ErrorCode> AcceptFriendReq(Int64 uid, Int64 friendUid)
    {
        try
        {
            var updateSuccess = await _queryFactory.Query("friend")
            .Where(q => q
                .Where("uid", uid)
                .Where("friend_uid", friendUid)
            )
            .OrWhere(q => q
                .Where("uid", friendUid)
                .Where("friend_uid", uid)
            )
            .UpdateAsync(new
            {
                friend_yn = 1
            });


            if (updateSuccess != 1)
            {
                _logger.ZLogError
                    ($"[AcceptFriendReq] ErrorCode: {ErrorCode.FailUpdateFriend}, uid: {uid}, friendUid{friendUid}");
                return ErrorCode.FailUpdateFriend;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError
                    ($"[AcceptFriendReq] ErrorCode: {ErrorCode.FailUpdateFriend}, uid: {uid}, friendUid{friendUid}");
            return ErrorCode.FailUpdateFriend;
        }

    }

    public async Task<ErrorCode> RejectFriendReq(Int64 uid, Int64 friendUid)
    {
        ErrorCode result = await DeleteFriend(uid, friendUid);

        return ErrorCode.None;
    }

    public async Task<(ErrorCode, Friend?)> GetFriemdDataByUidAndFriendUid(Int64 uid, Int64 friendUid)
    {
        try
        {
            dynamic? data = await _queryFactory.Query("friend")
            .Where(q => q
                .Where("uid", uid)
                .Where("friend_uid", uid)
            )
            .OrWhere(q => q
                .Where("uid", friendUid)
                .Where("friend_uid", uid)
            )
            .FirstOrDefaultAsync<Friend>();


            if (data == null)
            {
                _logger.ZLogError
                    ($"[GetFriemdDataByUidAndFriendUid] ErrorCode: {ErrorCode.NullFriendData}, uid: {uid}, friendUid: {friendUid}");
                return (ErrorCode.NullFriendData, null);
            }

            return (ErrorCode.None, data);
        }
        catch 
        {
            _logger.ZLogError
                    ($"[GetFriemdDataByUidAndFriendUid] ErrorCode: {ErrorCode.NullFriendData}, uid: {uid}, friendUid: {friendUid}");
            return (ErrorCode.NullFriendData, null);
        }
    }

    public async Task<(ErrorCode, IEnumerable<Friend?>?)> GetFriendListByUid(Int64 uid)
    {
        try
        {
            IEnumerable<Friend?> dataList = await _queryFactory.Query("friend")
                .Where("uid", uid).OrWhere("friend_uid", uid).GetAsync<Friend>();

            // 친구가 없어서 null인지 데이터를 가져오지 못해 null인지 판단 불가능
            _logger.ZLogInformation
                    ($"[GetFriendListByUid] Null Friend List, uid: {uid}");

            return (ErrorCode.None, dataList);
        }
        catch
        {
            _logger.ZLogError
                    ($"[GetFriendListByUid] ErrorCode: {ErrorCode.FailGetFrinedData}, uid: {uid}");
            return (ErrorCode.FailGetFrinedData, null);
        }

    }

    public async Task<(ErrorCode, Mail?)> GetMailByMailId(Int64 mailId)
    {
        // 메일 id로 메일 검색
        try
        {
            dynamic? data = await _queryFactory.Query("mail")
                .Where("mail_id", mailId).FirstOrDefaultAsync<Mail>();

            if(data == null) 
            {
                _logger.ZLogError
                    ($"[GetMailByMailId] ErrorCode: {ErrorCode.FailGetMail}, mailId: {mailId}");
                return (ErrorCode.FailGetMail, null);
            }

            return (ErrorCode.None, data);
        }
        catch
        {
            _logger.ZLogError
                    ($"[GetMailByMailId] ErrorCode: {ErrorCode.FailGetMail}, mailId: {mailId}");
            return (ErrorCode.FailGetMail, null);
        }
    }

    public async Task<(ErrorCode, IEnumerable<Mail?>?)> GetMailListByUid(Int64 uid)
    {
        // uid로 메일 리스트 검색
        try
        {
            IEnumerable<Mail?> dataList = await _queryFactory.Query("mail")
                .Where("uid", uid).GetAsync<Mail>();
            
            if(dataList == null)
            {
                _logger.ZLogError
                    ($"[GetMailByMailId] ErrorCode: {ErrorCode.FailGetMail}, uid: {uid}");
                return (ErrorCode.FailGetMail, null);
            }

            return (ErrorCode.None, dataList);
        }
        catch
        {
            _logger.ZLogError
                    ($"[GetMailByMailId] ErrorCode: {ErrorCode.FailGetMail}, uid: {uid}");
            return (ErrorCode.FailGetMail, null);
        }
    }

    public async Task<ErrorCode> DeleteMailByMailid(Int64 mailId)
    {
        // mailid로 검색하여 삭제
        try
        {
            int delSuccess = await _queryFactory.Query("mail")
                .Where("mail_id", mailId).DeleteAsync();
            
            if(delSuccess != 1)
            {
                _logger.ZLogError
                    ($"[GetMailByMailId] ErrorCode: {ErrorCode.FailGetMail}, mailId: {mailId}");
                return ErrorCode.FailDeleteMail;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError
                    ($"[GetMailByMailId] ErrorCode: {ErrorCode.FailGetMail}, mailId: {mailId}");
            return ErrorCode.FailDeleteMail;
        }
    }

    public async Task<ErrorCode> UpdateMailRewardedBymailId(Int64 mailId)
    {
        // mail id로 검색하여 수신여부 업데이트
        try
        {
            var updateSuccess = await _queryFactory.Query("mail")
                .Where("mail", mailId)
                .UpdateAsync(new
                {
                    receive_yn = true
                });

            if(updateSuccess != 1) 
            {
                _logger.ZLogError
                    ($"[UpdateMailRewardedBymailId] ErrorCode: {ErrorCode.FailReceiveMailReward}, mailId: {mailId}");
                return ErrorCode.FailReceiveMailReward;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError
                    ($"[UpdateMailRewardedBymailId] ErrorCode: {ErrorCode.FailReceiveMailReward}, mailId: {mailId}");
            return ErrorCode.FailReceiveMailReward;
        }
    }

    public async Task<(ErrorCode, IEnumerable<MailItem?>?)> GetMailItemListByMailId(Int64 mailId)
    {
        // mail id로 검색하여 보상 아이템 정보들 획득하기
        try
        {
            IEnumerable<MailItem?> dataList = await _queryFactory.Query("mail_item")
                .Where("mail_id", mailId).GetAsync<MailItem>();

            if (dataList == null)
            {
                _logger.ZLogError
                    ($"[GetMailItemListByMailId] ErrorCode: {ErrorCode.FailGetMailItemData}, mailId: {mailId}");
                return (ErrorCode.FailGetMailItemData, null);
            }

            return (ErrorCode.None, dataList);
        }
        catch
        {
            _logger.ZLogError
                    ($"[GetMailItemListByMailId] ErrorCode: {ErrorCode.FailGetMailItemData}, mailId: {mailId}");
            return (ErrorCode.FailGetMailItemData, null);
        }

    }

    public async Task<ErrorCode> CreateMailItem(Int64 mailId, Int64 ItemTemplateId, int itemCount)
    {
        // mail reward item 생성
        try
        {
            MailItem mailItem = new MailItem()
            {
                mail_id = mailId,
                item_template_id = ItemTemplateId,
                item_count = itemCount,
            };

            int insertSuccess = await _queryFactory.Query("mail_item")
                .InsertAsync(mailItem);

            if(insertSuccess != 1)
            {
                _logger.ZLogError
                    ($"[CreateMailItem] ErrorCode: {ErrorCode.FailCreateMailItem}, mailId: {mailId}");
                return ErrorCode.FailCreateMailItem;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError
                    ($"[CreateMailItem] ErrorCode: {ErrorCode.FailCreateMailItem}, mailId: {mailId}");
            return ErrorCode.FailCreateMailItem;
        }
    }

    public async Task<ErrorCode> CreateItem(Int64 uid, Int64 itemTemplateId, int itemCount)
    {
        // 아이템 테이블에 아이템 생성
        try
        {
            Item item = new Item()
            {
                item_template_id = itemTemplateId,
                owner_id = uid,
                item_count = itemCount,
            };

            int insertSuccess = await _queryFactory.Query("item")
                .InsertAsync(item);

            if (insertSuccess != 1)
            {
                _logger.ZLogError
                    ($"[CreateItem] ErrorCode: {ErrorCode.FailCreateMailItem}, uid: {uid}, itemTemplateId: {itemTemplateId}, itemCount: {itemCount}");
                return ErrorCode.FailCreateMailItem;
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError
                    ($"[CreateItem] ErrorCode: {ErrorCode.FailCreateMailItem}, uid: {uid}, itemTemplateId: {itemTemplateId}, itemCount: {itemCount}");
            return ErrorCode.FailCreateMailItem;
        }
    }

    public async Task<ErrorCode> CreateItemList(Int64 uid, IEnumerable<Item> itemList)
    {
        // item 여러개를 한번에 삽입
        try
        {
            foreach (Item? item in itemList)
            {
                int insertSuccess = await _queryFactory.Query("item").InsertAsync(item);
                if(insertSuccess != 1)
                {
                    _logger.ZLogError
                        ($"[CreateItem] ErrorCode: {ErrorCode.FailCreateItemList}, uid: {uid}");
                    return ErrorCode.FailCreateItemList;
                }
            }

            return ErrorCode.None;
        }
        catch
        {
            _logger.ZLogError
                ($"[CreateItem] ErrorCode: {ErrorCode.FailCreateItemList}, uid: {uid}");
            return ErrorCode.FailCreateItemList;
        }
    }

    public async Task<ErrorCode> CreateItemListByMailItemList(Int64 uid, IEnumerable<MailItem?>? itemList)
    {
        if(itemList == null)
        {
            _logger.ZLogError
                ($"[CreateItemListByMailItemList] ErrorCode: {ErrorCode.NullMailItemList}, uid: {uid}");
            return ErrorCode.FailCreateItemList;
        }


        // mail item을 item으로 전환하여 테이블에 삽입
        List<Item> items = new List<Item>();

        foreach(MailItem? mailitem in itemList)
        {
            if(mailitem == null)
            {
                _logger.ZLogError
                    ($"[CreateItemListByMailItemList] ErrorCode: {ErrorCode.NullMailItemList}, uid: {uid}");
                return ErrorCode.FailCreateItemList;
            }

            Item item = new Item() 
            { 
                item_template_id = mailitem.item_template_id,
                owner_id = uid,
                item_count = mailitem.item_count,
            };

            items.Add(item);
        }

        return await CreateItemList(uid, items);
    }

    public async Task<(ErrorCode, IEnumerable<Item>?)> GetItemListByUid(Int64 uid)
    {
        try
        {
            IEnumerable<Item> items = await _queryFactory.Query("Item")
                .Where("owner_id", uid).GetAsync<Item>();

            if(items == null) // 플레이어의 아이템이 존재하지 않는 것은 오류가 아니다.
            {
                _logger.ZLogInformation
                    ($"[GetItemListByUid] NullItemList, uid: {uid}");
            }

            return (ErrorCode.None, items);
        }
        catch
        {
            _logger.ZLogInformation
                ($"[GetItemListByUid] NullItemList, uid: {uid}");
            return (ErrorCode.FailGetItemList, null);
        }
    }

    
}


public class DbConfig
{
    public string GameDb { get; set; } = string.Empty;
}