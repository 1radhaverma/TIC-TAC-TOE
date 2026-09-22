using TicTacToe.Application.DTOs;
using TicTacToe.Application.Interfaces;
using TicTacToe.Application.Mapping;
using TicTacToe.Domain.Enums;

namespace TicTacToe.Application.Services;

/// <inheritdoc cref="IScoreboardService"/>
public class ScoreboardService : IScoreboardService
{
    private readonly IScoreboardRepository _scoreboardRepository;

    // Constructor injection of an interface, never a concrete repository class -
    // this class does not know or care whether the scoreboard lives in memory or
    // in a database (Dependency Inversion).
    public ScoreboardService(IScoreboardRepository scoreboardRepository)
    {
        _scoreboardRepository = scoreboardRepository;
    }

    public ScoreboardDto GetScoreboard() => GameMapper.ToScoreboardDto(_scoreboardRepository.Get());

    public ScoreboardDto ResetScoreboard()
    {
        var scoreboard = _scoreboardRepository.Get();
        scoreboard.Reset();
        return GameMapper.ToScoreboardDto(scoreboard);
    }

    public void RecordResult(GameStatus status, Mark? winner)
    {
        var scoreboard = _scoreboardRepository.Get();
        scoreboard.RecordResult(status, winner);
    }
}
