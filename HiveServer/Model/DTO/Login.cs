using System.ComponentModel.DataAnnotations;

// annotation 방식 이메일 형식 검사
namespace HiveServer.Model.DTO
{
    public class LoginReq
    {
        [Required(ErrorMessage = "EMAIL CANNOT BE EMPTY")]
        [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
        [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "PASSWORD CANNOT BE EMPTY")]
        [MinLength(1, ErrorMessage = "PASSWORD CANNOT BE EMPTY")]
        [StringLength(30, ErrorMessage = "PASSWORD IS TOO LONG")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRes
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
        public Int64 UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
