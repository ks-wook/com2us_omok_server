namespace GameAPIServer
{
    public class GameDefine
    {
        // 로그인 토큰
        public static short LoginTokenExpireMin = 10;

        // redis 트랜잭션용 키
        public static short ReqLockExpireMin = 1;
    }
}
