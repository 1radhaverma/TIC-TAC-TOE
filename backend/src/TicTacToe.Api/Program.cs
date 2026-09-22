using TicTacToe.Api.Infrastructure.Persistence;
using TicTacToe.Api.Middleware;
using TicTacToe.Application.Interfaces;
using TicTacToe.Application.Services;
using TicTacToe.Domain.Rules;
using TicTacToe.Domain.Strategies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();
builder.Services.AddSingleton<IScoreboardRepository, InMemoryScoreboardRepository>();


builder.Services.AddSingleton<IWinChecker, WinChecker>();
builder.Services.AddSingleton<IComputerPlayerStrategy, BasicComputerStrategy>();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IScoreboardService, ScoreboardService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string AngularDevClient = "AngularDevClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevClient, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(AngularDevClient);
app.MapControllers();

app.Run();

public partial class Program
{
}
