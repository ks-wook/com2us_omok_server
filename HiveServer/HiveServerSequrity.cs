using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HiveServer;


public class HiveServerSequrity
{
    /*
     * summary
     * salt값을 같이 넣어주는 경우 그 값으로 해싱
     * 그렇지 않은 경우 salt값을 생성 후 salt값과 해싱된 값을 같이 반환
     */

    // saltValue를 생성하여 해시값 생성
    public static (string saltString, string hashedString) HashingWithSaltValue(string textString)
    {
        byte[] generatedSalt = GenerateSaltValue();

        using (var sha256 = SHA256.Create())
        {
            // 평문과 salt를 연결하여 byte 배열로 변환
            byte[] saltedbyteArr = Encoding.UTF8.GetBytes(textString + Convert.ToBase64String(generatedSalt));

            string hashedString = Convert.ToBase64String(sha256.ComputeHash(saltedbyteArr));
            string saltString = Convert.ToBase64String(generatedSalt);

            return (saltString, hashedString);
        }
    }


    // 지정된 salt value로 해시값 생성
    public static (string saltString, string hashedString) HashingWithSaltValue(byte[] saltValue, string textString)
    {
        using (var sha256 = SHA256.Create())
        {
            // 평문과 salt를 연결하여 byte 배열로 변환
            byte[] saltedbyteArr = Encoding.UTF8.GetBytes(textString + Convert.ToBase64String(saltValue));

            string hashedString = Convert.ToBase64String(sha256.ComputeHash(saltedbyteArr));
            string saltString = Convert.ToBase64String(saltValue);

            return (saltString, hashedString);
        }
    }


    public static byte[] GenerateSaltValue()
    {
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
        return salt;
    }

    // 이메일 형식 체크
    public static bool IsValidEmail(string email)
    {
        // 이메일 주소 형식을 검사하기 위한 정규식 패턴
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }
}

