namespace GameAPIServer;

public enum ErrorCode : short
{
    None = 0,


    // User Authentication 2001 ~ 
    InvalidResFromHive = 2001,








    // Token 4001 ~
    NullGameLoginToken = 4002,


    // Login 5001 ~
    GameLoginFail = 5001,
    GameLoginTokenRedisFail = 5003,
    GameRedisConnectionFail = 5004,
}
