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


    // AccountDb 3001 ~ 
    NullAccountDbConnectionStr = 3001,
    InsertAccountFail = 3002,


    // Token 4001 ~
    NullServerToken = 4001,

    // Login 5001 ~
    LoginFail = 5001,
    EmailOrPasswordMismatch = 5002,
}

