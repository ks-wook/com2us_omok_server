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
        ErrorCode _result = ErrorCode.None;
        public ErrorCode Result
        {
            get { return _result; }
            set
            {
                _result = value;
                this.message = ErrorMessage.GetErrorMsg(_result);
            }
        }
        public string? message { get; set; }
    }
}
