using HiveServer;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HiveServer.Model.DAO
{
    public class CreateAccountReq
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class CreateAccountRes
    {
        public ErrorCode result { get; set; } = ErrorCode.None;
    }


}
