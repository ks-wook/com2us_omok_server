namespace HiveServer.Repository;


public static class MemoryDbKeyGenerator
{
    // 로그인
    readonly static string LoginTokenKey = "LoginToken_";

    // 로그인 토큰값은 accountId를 통해 key를 생성한다.
    public static string GenLoginTokenKey(string accountId) 
    {
        return LoginTokenKey + accountId;
    }
}
