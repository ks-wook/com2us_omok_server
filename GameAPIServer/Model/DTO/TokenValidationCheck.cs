namespace GameAPIServer.Model.DTO
{
    public class TokenValidationCheckReq
    {
        public Int64 accountId { get; set; }
        public string token { get; set; } = string.Empty;
    }

    public class TokenValidationCheckRes
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
    }
}
