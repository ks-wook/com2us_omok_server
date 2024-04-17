namespace GameAPIServer.Repository;

public class MemoryDb : IMemoryDb
{

}

public class MemoryDbConfig
{
    public string HiveRedis { get; set; }
}
