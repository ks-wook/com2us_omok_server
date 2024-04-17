using System.Diagnostics.Tracing;

namespace HiveServer;

public enum ErrorCode : short
{
    None = 0,


    // Account 2001 ~ 
    CreateAccountFail = 2001,
    InvalidEmailFormat = 2002,
    DuplicatedEmail = 2003,
    InvalidAccountEmail = 2004,
    NullAccountDbConnectionStr = 2005,
    InsertAccountFail = 2006,

    // Authentication 3001 ~
    TokenValidationCheckFail = 3001,
    TokenMismatch = 3002,


    // Token 4001 ~
    NullServerToken = 4001,

    // Login 5001 ~
    LoginFail = 5001,
    EmailOrPasswordMismatch = 5002,
    LoginTokenRedisFail = 5003,
    RedisConnectionFail = 5004,


}

