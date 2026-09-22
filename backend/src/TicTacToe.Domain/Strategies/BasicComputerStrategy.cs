using TicTacToe.Domain.Entities;
using TicTacToe.Domain.Enums;
using TicTacToe.Domain.Rules;

namespace TicTacToe.Domain.Strategies;


public class BasicComputerStrategy : IComputerPlayerStrategy
{
    private static readonly int[] Corners = { 0, 2, 6, 8 };
    private const int Center = 4;

    private readonly IWinChecker _winChecker;

    public BasicComputerStrategy(IWinChecker winChecker)
    {
        _winChecker = winChecker;
    }

    public int ChooseMove(Board board, Mark computerMark, Mark opponentMark)
    {
        // 1. Can the computer win right now?
        var winningMove = FindWinningMove(board, computerMark);
        if (winningMove.HasValue)
        {
            return winningMove.Value;
        }

        // 2. Can the opponent win on their next move? If so, take that cell to block it.
        var blockingMove = FindWinningMove(board, opponentMark);
        if (blockingMove.HasValue)
        {
            return blockingMove.Value;
        }

        // 3. Center is the strongest remaining cell (part of 4 winning lines).
        if (board.IsCellEmpty(Center))
        {
            return Center;
        }

        // 4. Corners are the next strongest (each part of 3 winning lines).
        var openCorner = Corners.FirstOrDefault(board.IsCellEmpty, -1);
        if (openCorner != -1)
        {
            return openCorner;
        }

        // 5. Anything left - edges are the last resort.
        for (var i = 0; i < Board.CellCount; i++)
        {
            if (board.IsCellEmpty(i))
            {
                return i;
            }
        }

        // The caller (GameService) only invokes this when the board is not full and
        // not already won, so reaching here means a caller broke that contract.
        throw new InvalidOperationException("No empty cell available for the computer to play.");
    }

    /// <summary>
    /// Tries every empty cell as a hypothetical move for <paramref name="mark"/> and
    /// returns the first one that would complete a winning line, or null if none does.
    /// A cloned board is used for each trial so the real board is never disturbed.
    /// </summary>
    private int? FindWinningMove(Board board, Mark mark)
    {
        for (var i = 0; i < Board.CellCount; i++)
        {
            if (!board.IsCellEmpty(i))
            {
                continue;
            }

            var trial = board.Clone();
            trial.PlaceMark(i, mark);

            if (_winChecker.Evaluate(trial).HasWinner)
            {
                return i;
            }
        }

        return null;
    }
}
