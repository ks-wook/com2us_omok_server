using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace APIAccountServer;


public class HiveServerSequrity
{

    // TODO 해쉬 토큰 생성기





    // TODO 비밀번호 복호화 함수



    static bool IsValidEmail(string email)
    {
        // 이메일 주소 형식을 검사하기 위한 정규식 패턴
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }
}

