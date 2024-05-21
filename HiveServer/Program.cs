using HiveServer.Services;
using SqlKata;
using SqlKata.Execution;
using ZLogger;
using Microsoft.Extensions.Logging;
using HiveServer.Repository;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration;

// Service Init, DI
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.Configure<MemoryDbConfig>(configuration.GetSection(nameof(MemoryDbConfig)));

builder.Services.AddTransient<IHiveDb,  HiveDb>();
builder.Services.AddSingleton<IMemoryDb,  MemoryDb>();
builder.Services.AddControllers();

builder.WebHost.UseUrls("http://*:5014");


// Logger Setting
Host.CreateDefaultBuilder()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();

        logging.AddZLoggerConsole();

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



app.Run();
