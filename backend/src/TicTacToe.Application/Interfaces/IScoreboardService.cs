using TicTacToe.Application.DTOs;
using TicTacToe.Domain.Enums;

namespace TicTacToe.Application.Interfaces;


public interface IScoreboardService
{
    ScoreboardDto GetScoreboard();

    ScoreboardDto ResetScoreboard();

    /// <summary>
    /// Records the outcome of one completed game. Called internally by
    /// <see cref="IGameService"/> - never directly by a controller - exactly
    /// once per game, right when that game's status becomes Won or Draw.
    /// </summary>
    void RecordResult(GameStatus status, Mark? winner);
}
