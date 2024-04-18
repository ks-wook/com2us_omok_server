namespace GameAPIServer.Repository;

public static class MemoryDbKeyGenerator
{
    // 로그인
    readonly static string LoginTokenKey = "UID_";

    public static string GenLoginTokenKey(string accountId)
    {
        return LoginTokenKey + accountId;
    }
}
