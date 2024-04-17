using HiveServer;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HiveServer.Model.DTO
{
    public class CreateAccountReq
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class CreateAccountRes
    {
        public ErrorCode result { get; set; } = ErrorCode.None;
    }
}
