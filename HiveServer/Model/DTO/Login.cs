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
        public ErrorCode Result { get; set; } = ErrorCode.None;
        public Int64 AccountId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
