using APIAccountServer;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HiveServer.Model.DAO
{
    public class CreateAccountReq
    {
        [Required, NotNull]
        public string email { get; set; }

        [Required, NotNull]
        public string password { get; set; }
    }

    public class CreateAccountRes
    {
        public ErrorCode result { get; set; } = ErrorCode.None;
    }


}
