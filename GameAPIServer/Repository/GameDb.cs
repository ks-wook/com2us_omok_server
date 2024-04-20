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

    public GameDb(ILogger<GameDb> logger, IOptions<DbConfig> dbConfig)
    {
        _dbConfig = dbConfig;
        _logger = logger;

        _dbCompiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConnector, _dbCompiler);
    }


    // create connection with db
    private ErrorCode DbConnect()
    {
        string? connectionStr = _dbConfig.Value.GameDb;
        if (connectionStr == null) // db 연결문자열 가져오기 실패
        {
            _logger.ZLogError(
                $"[DbConnect] Null Db Connection String");

            return ErrorCode.NullAccountDbConnectionStr;
        }

        _dbConnector = new MySqlConnection(connectionStr);
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
        // 새로운 게임 데이터 삽입
        UserGameData? userGameData = new UserGameData()
        {
            account_id = accountId,
            nickname = "User" + accountId,
        };

        try
        {
            var insertSuccess = await _queryFactory.Query("user_game_data")
            .InsertAsync(userGameData);

            _logger.ZLogDebug
                ($"[CreateUserGameData] accountId: {accountId}, nickname: {userGameData.nickname}");


            if (insertSuccess != 1)
            {
                _logger.ZLogError
                    ($"[CreateUserGameData] ErrorCode: {ErrorCode.FailCreateNewGameData}, " +
                     $"accountId: {accountId}, nickname: {userGameData.nickname}");
                return (ErrorCode.FailCreateNewGameData, null);
            }


            // 데이터 삽입 성공, 삽입된 게임데이터 획득
            (ErrorCode result, userGameData) = await GetUserGameDataByAccountId(accountId);

            return (result, userGameData);
        }
        catch
        {
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
                ($"[CreateUserGameData] ErrorCode: {ErrorCode.FailCreateNewGameData}, accountId: {accountId}");
            return (ErrorCode.NullUserGameData, null);
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
                    ($"[GetFriendListByUid] ErrorCode: {ErrorCode.FailGetFrinedData}, uid: {uid}, friendUid: {friendUid}");
            return (ErrorCode.FailGetFrinedData, null);
        }

    }

}


public class DbConfig
{
    public string GameDb { get; set; } = string.Empty;
}