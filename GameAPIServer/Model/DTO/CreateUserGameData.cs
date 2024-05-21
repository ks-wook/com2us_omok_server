namespace GameAPIServer.Model.DTO;

public class CreateUserGameDataReq
{
    public Int64 AccountId { get; set; }
}

public class CreateUserGameDataRes
{
    public ErrorCode result { get; set; } = ErrorCode.None;
}