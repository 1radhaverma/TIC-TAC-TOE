using TicTacToe.Domain.Enums;

namespace TicTacToe.Domain.Entities;

/// <summary>
/// Session-level tally of results across every completed game.
///
/// Deliberately its own entity rather than fields bolted onto <see cref="Game"/>:
/// a scoreboard outlives any single game (Reset Game must NOT touch it, only
/// Reset Scoreboard may), so giving it its own identity and lifecycle is a
/// direct application of Single Responsibility - "count results" is a different
/// job from "manage one game's rules and state".
/// </summary>
public class Scoreboard
{
    public int XWins { get; private set; }
    public int OWins { get; private set; }
    public int Draws { get; private set; }

    /// <summary>
    /// Records the outcome of exactly one completed game. Only <see cref="GameStatus.Won"/>
    /// and <see cref="GameStatus.Draw"/> are meaningful outcomes; an in-progress game has
    /// nothing to record yet, so that case is a no-op rather than an error - callers
    /// (see GameService) only call this once, right when a game *becomes* completed.
    /// </summary>
    public void RecordResult(GameStatus status, Mark? winner)
    {
        switch (status)
        {
            case GameStatus.Won when winner == Mark.X:
                XWins++;
                break;
            case GameStatus.Won when winner == Mark.O:
                OWins++;
                break;
            case GameStatus.Draw:
                Draws++;
                break;
        }
    }

    /// <summary>Clears every counter back to zero. Used only by the explicit "Reset Scoreboard" action.</summary>
    public void Reset()
    {
        XWins = 0;
        OWins = 0;
        Draws = 0;
    }
}
