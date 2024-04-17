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
        ErrorCode _result = ErrorCode.None;
        public ErrorCode Result { 
            get { return _result; }
            set 
            {
                _result = value;
                this.message = ErrorMessage.GetErrorMsg(_result);
            } 
        }
        public Int64 accountId { get; set; }
        public string? LoginToken { get; set; }
        public string? message { get; set; }
    }
}
