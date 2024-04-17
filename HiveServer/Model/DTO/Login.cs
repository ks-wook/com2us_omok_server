using System.ComponentModel.DataAnnotations;

namespace HiveServer.Model.DTO
{
    public class LoginReq
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginRes
    {
        ErrorCode _result = ErrorCode.None;
        public ErrorCode result { 
            get { return _result; }
            set 
            {
                _result = value;
                this.message = ErrorMessage.GetErrorMsg(_result);
            } 
        }
        public Int64? accountId { get; set; }
        public string? accountToken { get; set; }
        public string? message { get; set; }
    }
}
