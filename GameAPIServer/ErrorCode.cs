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

    // Token 14001 ~
    NullGameLoginToken = 14002,


    // Login 15001 ~
    GameLoginFail = 15001,
    GameLoginTokenRedisFail = 15002,
    GameRedisConnectionFail = 15003,
}
