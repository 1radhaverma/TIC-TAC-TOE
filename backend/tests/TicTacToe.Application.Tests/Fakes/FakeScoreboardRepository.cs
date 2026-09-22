using TicTacToe.Application.Interfaces;
using TicTacToe.Domain.Entities;

namespace TicTacToe.Application.Tests.Fakes;

/// <summary>A minimal in-memory <see cref="IScoreboardRepository"/> used only by tests.</summary>
public class FakeScoreboardRepository : IScoreboardRepository
{
    private readonly Scoreboard _scoreboard = new();

    public Scoreboard Get() => _scoreboard;
}
