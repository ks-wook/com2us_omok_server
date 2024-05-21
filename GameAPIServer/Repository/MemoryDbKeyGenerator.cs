namespace GameAPIServer.Repository;

public static class MemoryDbKeyGenerator
{
    // 로그인
    readonly static string LoginTokenKey = "LoginToken_";
    readonly static string UserLockKey = "UserLock_";

    // 로그인 토큰값은 uid 통해 key를 생성한다.
    public static string GenLoginTokenKey(string uid)
    {
        return LoginTokenKey + uid;
    }

    public static string GenUserLockKey(string uid)
    {
        return UserLockKey + uid;
    }
}
