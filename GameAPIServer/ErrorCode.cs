namespace GameAPIServer;

public enum ErrorCode : short
{
    None = 0,


    // User Authentication 12001 ~ 
    InvalidResFromHive = 12001,


    // GameDb 13001 ~
    NullAccountDbConnectionStr = 13001,
    NullUserGameData = 13002,
    FailCreateNewGameData = 13003,
    FailDisconnectGameDb = 13004,
    FailCreateNewFriendData = 13005,
    FailDeleteFriend = 13006,
    FailUpdateFriend = 13007,
    NullFriendData =  13008,
    FailGetFrinedData = 13009,
    FailRejectFriendReq = 13010,
    FailGetMail = 13011,
    FailReceiveMailReward = 13012,
    FailDeleteMail = 13013,
    FailGetMailItemData = 13014,
    FailCreateMailItem = 13015,
    FailCreateItem = 13016,
    FailCreateItemList = 13017,
    NullMailItemList = 13018,
    FailGetItemList = 13019,
    FailGetUserGameData = 13020,

    // Token 14001 ~
    NullGameLoginToken = 14002,


    // Login 15001 ~
    GameLoginFail = 15001,
    GameLoginTokenRedisFail = 15002,
    GameRedisConnectionFail = 15003,
}
