namespace GameAPIServer;

public enum ErrorCode : short
{
    None = 0,


    // User Authentication 12001 ~ 
    InvalidResFromHive = 12001,








    // Token 14001 ~
    NullGameLoginToken = 14002,


    // Login 15001 ~
    GameLoginFail = 15001,
    GameLoginTokenRedisFail = 15002,
    GameRedisConnectionFail = 15003,
}
