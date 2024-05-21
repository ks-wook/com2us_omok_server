namespace HiveServer.Model.DTO;

public class UserIdReq
{
    public Int64 AccountId { get; set; } = 0;
}

public class UserIdRes
{
    public ErrorCode result { get; set; } = ErrorCode.None;
    public Int64 uid { get; set; } = 0;
}