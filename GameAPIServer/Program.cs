using GameAPIServer.Repository;
using GameAPIServer.Services;
using Microsoft.Extensions.Logging;
using ZLogger;

var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration;

// Service Init, DI
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.Configure<MemoryDbConfig>(configuration.GetSection(nameof(MemoryDbConfig)));

builder.Services.AddTransient<IGameDb, GameDb>();
builder.Services.AddSingleton<IMemoryDb, MemoryDb>();
builder.Services.AddTransient<IGameService, GameService>();
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddControllers();


// Logger Setting
Host.CreateDefaultBuilder()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();

        // TEST �ֿܼ� �α� ���
        logging.AddZLoggerConsole();

        // TODO ��¥�� ���� ���Ͽ� �α� ���
        // logging.AddZLoggerFile("fileName.log");
        // ��¥�� ���� �ٸ� ������ �����Ͽ� �α��� ����ȭ
        // logging.AddZLoggerRollingFile((dt, x) => $"logs/{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log", x => x.ToLocalTime().Date, 1024);

        logging.AddZLoggerConsole(options =>
        {
            options.UseJsonFormatter();
        });
    });



var app = builder.Build();



app.UseRouting();

#pragma warning disable ASP0014
app.UseEndpoints(endpoints => { _ = endpoints.MapControllers(); });
#pragma warning restore ASP0014



app.Run(configuration["GameAPIServerAddr"]);
