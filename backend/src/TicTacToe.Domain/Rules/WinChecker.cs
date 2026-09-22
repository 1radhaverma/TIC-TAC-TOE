using TicTacToe.Domain.Entities;
using TicTacToe.Domain.Enums;

namespace TicTacToe.Domain.Rules;

/// <summary>
/// Standard 3x3 Tic Tac Toe win rule: 3 rows + 3 columns + 2 diagonals = 8 possible lines.
/// This is the *only* class in the whole solution that knows those 8 lines - if the
/// board size ever changed, this is the one place that would need to change
/// (Single Responsibility).
/// </summary>
public class WinChecker : IWinChecker
{
    // Each inner array is a set of 3 cell indices that together form a winning line.
    private static readonly int[][] WinningLines =
    {
        new[] { 0, 1, 2 }, // top row
        new[] { 3, 4, 5 }, // middle row
        new[] { 6, 7, 8 }, // bottom row
        new[] { 0, 3, 6 }, // left column
        new[] { 1, 4, 7 }, // middle column
        new[] { 2, 5, 8 }, // right column
        new[] { 0, 4, 8 }, // top-left to bottom-right diagonal
        new[] { 2, 4, 6 }  // top-right to bottom-left diagonal
    };

    public WinResult Evaluate(Board board)
    {
        foreach (var line in WinningLines)
        {
            var first = board.GetCell(line[0]);
            if (first == Mark.Empty)
            {
                continue;
            }

            if (board.GetCell(line[1]) == first && board.GetCell(line[2]) == first)
            {
                return WinResult.For(first, line);
            }
        }

        return WinResult.None;
    }
}
