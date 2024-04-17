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



        // TODO 에러 메시지 정리






    };

    public static string GetErrorMsg(ErrorCode errorCode)
    {
        return _dict[errorCode];
    }
}
