using HiveServer;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HiveServer.Model.DTO
{
    public class CreateAccountReq
    {
        [Required(ErrorMessage = "EMAIL CANNOT BE EMPTY")]
        [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
        [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "PASSWORD CANNOT BE EMPTY")]
        [MinLength(1, ErrorMessage = "PASSWORD CANNOT BE EMPTY")]
        [StringLength(30, ErrorMessage = "PASSWORD IS TOO LONG")]
        public string Password { get; set; } = string.Empty;
    }

    public class CreateAccountRes
    {
        public ErrorCode result { get; set; } = ErrorCode.None;
    }
}
