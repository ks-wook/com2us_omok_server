using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HiveServer;


public class HiveServerSequrity
{

    // TODO 해쉬 토큰 생성기





    public static (string saltString, string hashedPasswordString) HashPasswordWithSalt(string password)
    {
        byte[] salt = GenerateSalt();

        using (var sha256 = SHA256.Create())
        {
            // 비밀번호와 salt를 연결하여 byte 배열로 변환
            byte[] saltedPassword = Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt));

            string hashedPasswordString = Convert.ToBase64String(sha256.ComputeHash(saltedPassword));
            string saltString = Convert.ToBase64String(salt);

            return (saltString, hashedPasswordString);
        }
    }


    private static byte[] GenerateSalt()
    {
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
        return salt;
    }


    public static bool IsValidEmail(string email)
    {
        // 이메일 주소 형식을 검사하기 위한 정규식 패턴
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }
}

