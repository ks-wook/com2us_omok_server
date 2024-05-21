using GameAPIServer.Model.DAO.MemoryDb;
using GameAPIServer.Repository;

namespace GameAPIServer.Middleware;

public class UserTokenValidationCheck
{
    // 유저 토큰 유효성 검증 미들웨어
    readonly IMemoryDb _memoryDb;
    readonly RequestDelegate _next;

    public UserTokenValidationCheck(RequestDelegate next, IMemoryDb memoryDb)
    {
        _next = next;
        _memoryDb = memoryDb;
    }

    public async Task Invoke(HttpContext context)
    {
        // 로그인 전에 실행되는 요청에 대해서는 토큰 체크를 하지 않는다.
        var apiFormatStr = context.Request.Path.Value;
        if(string.Compare(apiFormatStr, "/Login", StringComparison.OrdinalIgnoreCase) == 0 ||
           string.Compare(apiFormatStr, "/GetUserId", StringComparison.OrdinalIgnoreCase) == 0 ||
           string.Compare(apiFormatStr, "/CreateUserGameData", StringComparison.OrdinalIgnoreCase) == 0)
        {
            await _next(context); // 다음작업으로 이동

            return;
        }

        // 토큰이 존재하는가?
        (bool isTokenExistInHeader, string? token) = await IsTokenExistInHeader(context);
        if(isTokenExistInHeader == false || token == null)
        {
            return;
        }

        // uid가 존재하는가?
        (bool isUidExistInHeader, string? uid) = await IsUidExistInHeader(context);
        if (isUidExistInHeader == false || uid == null)
        {
            return;
        }

        // uid에 대한 토큰값이 일치하는가?
        bool isTokenMatch = await IsTokenMatch(context, uid, token);
        if (isTokenMatch == false)
        {
            return;
        }

        // 토큰 검사 성공 요청에 대한 작업 수행

        // redis 키값 잠금
        string userLockKey = MemoryDbKeyGenerator.GenUserLockKey(uid);
        if(await SetUserLock(context, userLockKey) == false) // 이미 잠겨있거나 오류가 있다면 에러메시지 전송
        {
            return;
        }

        await _next(context); // 요청에 대한 작업 진행

        // 다음 과정을 모두 진행하고 난 후에 트랜잭션 해제
        await _memoryDb.DelUserReqLockAsync(userLockKey);
    }

    async Task<(bool, string?)> IsTokenExistInHeader(HttpContext context)
    {
        // 헤더에 토큰이 포함되어 있는가
        if (context.Request.Headers.TryGetValue("token", out var token))
        {
            return (true, token);
        }

        // 토큰이 없는 요청인경우
        HttpErrorMsg httpErrorMsg = new HttpErrorMsg
        {
            result = ErrorCode.TokenNotExistInHeader
        };

        await context.Response.WriteAsJsonAsync(httpErrorMsg);

        return (false, null);
    }

    async Task<(bool, string?)> IsUidExistInHeader(HttpContext context) 
    {
        // 헤더에 uid가 포함되어 있는가
        if (context.Request.Headers.TryGetValue("uid", out var uid))
        {
            return (true, uid);
        }

        // 토큰이 없는 요청인경우
        HttpErrorMsg httpErrorMsg = new HttpErrorMsg
        {
            result = ErrorCode.UidNotExsistInHeader
        };

        await context.Response.WriteAsJsonAsync(httpErrorMsg);

        return (false, null);
    }

    async Task<bool> IsTokenMatch(HttpContext context, string uid, string token) 
    {
        // uid에 해당하는 토큰이 일치하는가?
        (ErrorCode result, LoginToken? searchedToken) = await _memoryDb.GetGameTokenByUidAsync(long.Parse(uid));

        // uid에 해당하는 토큰이 존재하지 않는 경우
        if (result != ErrorCode.None || searchedToken == null)
        {
            // 오류 메시지 전송
            HttpErrorMsg httpErrorMsg = new HttpErrorMsg
            {
                result = ErrorCode.NullGameLoginToken
            };

            await context.Response.WriteAsJsonAsync(httpErrorMsg);

            return false;
        }

        if (string.CompareOrdinal(token, searchedToken.Token) != 0) // 토큰이 일치하지 않는 경우
        {
            // 오류 메시지 전송
            HttpErrorMsg httpErrorMsg = new HttpErrorMsg
            {
                result = ErrorCode.TokenMismatch
            };

            await context.Response.WriteAsJsonAsync(httpErrorMsg);

            return false;
        }

        return true; 
    }

    async Task<bool> SetUserLock(HttpContext context, string userLockKey)
    {
        if(await _memoryDb.SetUserReqLockAsync(userLockKey) != ErrorCode.None)
        {
            HttpErrorMsg httpErrorMsg = new HttpErrorMsg
            {
                result = ErrorCode.FailSetUserLockKey
            };

            await context.Response.WriteAsJsonAsync(httpErrorMsg);

            return false;
        }

        return true;
    }
}

public class HttpErrorMsg
{
    public ErrorCode result { get; set; } = ErrorCode.None;
}