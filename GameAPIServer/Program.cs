using GameAPIServer.Middleware;
using GameAPIServer.Repository;
using GameAPIServer.Services;
using Microsoft.Extensions.Logging;
using System.Net;
using ZLogger;

var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration;

// Service Init, DI
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.Configure<MemoryDbConfig>(configuration.GetSection(nameof(MemoryDbConfig)));

builder.Services.AddTransient<IGameDb, GameDb>();
builder.Services.AddSingleton<IMemoryDb, MemoryDb>();
builder.Services.AddTransient<IGameService, GameService>();
builder.Services.AddTransient<IFriendService, FriendService>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddControllers();

builder.WebHost.UseUrls("http://*:5015");

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

app.UseMiddleware<UserTokenValidationCheck>();

app.MapDefaultControllerRoute();

app.Run();