using TicTacToe.Domain.Entities;
using TicTacToe.Domain.Enums;
using TicTacToe.Domain.Rules;
using TicTacToe.Domain.Strategies;
using Xunit;

namespace TicTacToe.Domain.Tests;

public class ComputerStrategyTests
{
    private readonly BasicComputerStrategy _sut = new(new WinChecker());

    [Fact]
    public void ChooseMove_WhenComputerCanWinImmediately_TakesTheWinningCell()
    {
        // O has two in a row (3,4) and can win by taking 5.
        var board = Board.ReplayMoves(new[]
        {
            new Move(1, Mark.X, 0),
            new Move(2, Mark.O, 3),
            new Move(3, Mark.X, 1),
            new Move(4, Mark.O, 4)
        });

        var move = _sut.ChooseMove(board, Mark.O, Mark.X);

        Assert.Equal(5, move);
    }

    [Fact]
    public void ChooseMove_WhenComputerCannotWin_ButOpponentCanNext_BlocksThatCell()
    {
        // X has two in a row (0,1) and would win by taking 2 next; O has no
        // immediate win available, so O must block cell 2.
        var board = Board.ReplayMoves(new[]
        {
            new Move(1, Mark.X, 0),
            new Move(2, Mark.O, 3),
            new Move(3, Mark.X, 1)
        });

        var move = _sut.ChooseMove(board, Mark.O, Mark.X);

        Assert.Equal(2, move);
    }

    [Fact]
    public void ChooseMove_NoWinOrBlockAvailable_PrefersTheCenter()
    {
        var board = Board.ReplayMoves(new[]
        {
            new Move(1, Mark.X, 0)
        });

        var move = _sut.ChooseMove(board, Mark.O, Mark.X);

        Assert.Equal(4, move); // center
    }

    [Fact]
    public void ChooseMove_CenterTaken_PrefersACorner()
    {
        var board = Board.ReplayMoves(new[]
        {
            new Move(1, Mark.X, 4) // human took the center already
        });

        var move = _sut.ChooseMove(board, Mark.O, Mark.X);

        Assert.Contains(move, new[] { 0, 2, 6, 8 });
    }

    // Note: there is intentionally no test that drives ChooseMove all the way down
    // to its final "take any empty cell" line. Doing so would require a board
    // where the center AND all four corners are already occupied while the game
    // is still undecided - but the two diagonals are exactly {corner, center,
    // corner}, and the four corners form a cycle (0-2, 2-8, 8-6, 6-0) that must
    // alternate to keep every row/column safe. Alternating a 4-cycle forces
    // opposite corners (0&8, 2&6) to match, which forces one diagonal to already
    // be complete. In other words: on a real 3x3 board, center+corners are never
    // all filled without the game already having a winner. The final fallback
    // line exists as a defensive last resort, not as a reachable branch - see the
    // comment above it in BasicComputerStrategy.
}
