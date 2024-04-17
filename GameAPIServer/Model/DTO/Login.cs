using System.ComponentModel.DataAnnotations;

namespace GameAPIServer.Model.DTO
{
    public class LoginReq
    {
        [Required]
        public Int64 AccountId1 { get; set; }

        [Required]
        public string token { get; set; } = string.Empty;
    }

    public class LoginRes
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
        public string? LoginToken { get; set; }
    }
}
