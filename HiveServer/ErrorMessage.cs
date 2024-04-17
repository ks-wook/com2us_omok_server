namespace HiveServer;

public static class ErrorMessage
{

    readonly static Dictionary<ErrorCode, string> _dict = new Dictionary<ErrorCode, string>()
    {
        { 0, "" },


        // Account 2001 ~ 
        { ErrorCode.CreateAccountFail, "계정 생성에 실패하였습니다." },
        { ErrorCode.InvalidEmailFormat, "잘못된 이메일 형식입니다." },
        { ErrorCode.DuplicatedEmail, "이미 가입된 이메일 입니다." },
        { ErrorCode.InvalidAccountEmail, "하이브 계정이 존재하지 않습니다." },



        // AccountDb 3001 ~ 
        { ErrorCode.NullAccountDbConnectionStr, "DB 연결 문자열이 존재하지 않습니다." },
        { ErrorCode.InsertAccountFail, "계정 삽입에 실패하였습니다." },



        // Token 4001 ~
        { ErrorCode.NullServerToken, "서버 토큰이 존재하지 않습니다." },



        // Login 5001 ~
        { ErrorCode.LoginFail, "로그인에 실패하였습니다." },
        { ErrorCode.EmailOrPasswordMismatch, "이메일 혹은 비밀번호가 일치하지 않습니다." },


    };

    public static string GetErrorMsg(ErrorCode errorCode)
    {
        return _dict[errorCode];
    }
}
