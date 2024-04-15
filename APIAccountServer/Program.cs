using SqlKata;
using SqlKata.Execution;



var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration; // Config data

// Init?
// Service -> builder 
// Middleware -> app

// TODO Service Init
builder.Services.AddControllers();



// TODO Log Setting





var app = builder.Build();




// TODO Middleware Setting





app.UseRouting();

// Controller Mapping
#pragma warning disable ASP0014
app.UseEndpoints(endpoints => { _ = endpoints.MapControllers(); });
#pragma warning restore ASP0014




app.Run(configuration["APIAccountServerAddr"]);
