using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using APIServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;
builder.Services.Configure<MatchingConfig>(configuration.GetSection(nameof(MatchingConfig)));

builder.Services.AddSingleton<IMatchWoker, MatchWoker>();
builder.Services.AddControllers();

builder.WebHost.UseUrls("http://*:5114");

WebApplication app = builder.Build();

app.MapDefaultControllerRoute();


app.Run(configuration["MatchAPIServer"]);