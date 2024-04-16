using System.Diagnostics.Tracing;

namespace HiveServer;

public enum ErrorCode : short
{
    None = 0,


    // Account 2001 ~ 
    CreateAccountFail = 2001,
    InvalidEmailFormat = 2002,
    DuplicatedEmail = 2003,



    // AccountDb 3001 ~ 
    NullAccountDbConnectionStr = 3001,
    InsertAccountFail = 3002,

}

