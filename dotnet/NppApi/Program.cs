using NppApi.Hubs;
using NppApi.Services;
using NppCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSignalR();
builder.Services.AddSingleton<GameManager>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<GameManager>());

builder.Services.AddCassandraService(builder.Configuration);
builder.Services.AddRedisService(builder.Configuration);
builder.Services.AddGameStateRepository();
builder.Services.AddPlayerService();
builder.Services.AddAuthService();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();


app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");

app.Run();
