using TicTacToe.Domain.Entities;

namespace TicTacToe.Application.Interfaces;


public interface IScoreboardRepository
{
    /// <summary>Returns the current scoreboard, creating an empty one on first access.</summary>
    Scoreboard Get();
}
