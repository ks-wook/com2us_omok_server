using HiveServer.Services;
using SqlKata;
using SqlKata.Execution;
using ZLogger;


var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration;

// Service Init, DI
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));

builder.Services.AddTransient<IHiveDb,  HiveDb>();
builder.Services.AddControllers();


// Logger Setting
Host.CreateDefaultBuilder()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();

        // TEST 콘솔에 로그 출력
        logging.AddZLoggerConsole();

        // TODO 날짜에 따라 파일에 로그 출력
        // logging.AddZLoggerFile("fileName.log");
        // 날짜에 따라 다른 파일을 생성하여 로깅을 구조화
        // logging.AddZLoggerRollingFile((dt, x) => $"logs/{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log", x => x.ToLocalTime().Date, 1024);

        logging.AddZLoggerConsole(options =>
        {
            options.UseJsonFormatter();
        });
    });



var app = builder.Build();




// TODO Middleware Setting





app.UseRouting();

#pragma warning disable ASP0014
app.UseEndpoints(endpoints => { _ = endpoints.MapControllers(); });
#pragma warning restore ASP0014



app.Run(configuration["HiveServerAddr"]);
