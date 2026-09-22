using TicTacToe.Domain.Enums;

namespace TicTacToe.Domain.Rules;

/// <summary>
/// The outcome of asking "has anyone won this board yet?".
/// A small immutable value object instead of two loose out-parameters
/// (winner + winning cells) - it can be returned, stored and passed around as one unit.
/// </summary>
public record WinResult(bool HasWinner, Mark Winner, IReadOnlyList<int>? WinningCells)
{
    public static readonly WinResult None = new(false, Mark.Empty, null);

    public static WinResult For(Mark winner, IReadOnlyList<int> winningCells) => new(true, winner, winningCells);
}
