using System;

// 1000 ~ 19999
public enum ErrorCode : short
{
    None = 0,

    AythCheckFail = 21,
    ReceiptCheckFail = 22,

    // Match 15501 ~
    FailRequestMatch = 15501,
}