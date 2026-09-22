using TicTacToe.Application.Interfaces;
using TicTacToe.Domain.Entities;

namespace TicTacToe.Api.Infrastructure.Persistence;

public class InMemoryScoreboardRepository : IScoreboardRepository
{
    // A single mutable instance is created once and handed out on every call -
    // Scoreboard's own methods (RecordResult/Reset) are what keep it consistent,
    // this repository just holds the one instance alive for the process lifetime.
    private readonly Scoreboard _scoreboard = new();

    public Scoreboard Get() => _scoreboard;
}
