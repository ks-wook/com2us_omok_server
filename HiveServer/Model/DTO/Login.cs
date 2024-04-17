using System.ComponentModel.DataAnnotations;

namespace HiveServer.Model.DTO
{
    public class LoginReq
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRes
    {
        public ErrorCode result { get; set; } = ErrorCode.None;
        public Int64 accountId { get; set; }
        public string? LoginToken { get; set; }
        public string? message { get; set; }
    }
}
