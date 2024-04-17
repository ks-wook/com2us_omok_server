namespace HiveServer.Model.DTO
{
    public class TokenValidationCheckReq
    {
        public Int64 AccountId { get; set; }
        public string Token { get; set; } = string.Empty;
    }

    public class TokenValidationCheckRes
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
    }
}
