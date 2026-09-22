using TicTacToe.Domain.Entities;
using TicTacToe.Domain.Enums;
using TicTacToe.Domain.Rules;
using Xunit;

namespace TicTacToe.Domain.Tests;

public class WinCheckerTests
{
    private readonly WinChecker _sut = new(); // "sut" = system under test

    [Fact]
    public void Evaluate_TopRowWin_IsDetected()
    {
        var board = Board.ReplayMoves(new[]
        {
            new Move(1, Mark.X, 0),
            new Move(2, Mark.O, 3),
            new Move(3, Mark.X, 1),
            new Move(4, Mark.O, 4),
            new Move(5, Mark.X, 2) // completes top row: 0,1,2
        });

        var result = _sut.Evaluate(board);

        Assert.True(result.HasWinner);
        Assert.Equal(Mark.X, result.Winner);
        Assert.Equal(new[] { 0, 1, 2 }, result.WinningCells);
    }

    [Fact]
    public void Evaluate_LeftColumnWin_IsDetected()
    {
        var board = Board.ReplayMoves(new[]
        {
            new Move(1, Mark.O, 0),
            new Move(2, Mark.X, 1),
            new Move(3, Mark.O, 3),
            new Move(4, Mark.X, 2),
            new Move(5, Mark.O, 6) // completes left column: 0,3,6
        });

        var result = _sut.Evaluate(board);

        Assert.True(result.HasWinner);
        Assert.Equal(Mark.O, result.Winner);
        Assert.Equal(new[] { 0, 3, 6 }, result.WinningCells);
    }

    [Fact]
    public void Evaluate_DiagonalWin_IsDetected()
    {
        var board = Board.ReplayMoves(new[]
        {
            new Move(1, Mark.X, 0),
            new Move(2, Mark.O, 1),
            new Move(3, Mark.X, 4),
            new Move(4, Mark.O, 2),
            new Move(5, Mark.X, 8) // completes diagonal: 0,4,8
        });

        var result = _sut.Evaluate(board);

        Assert.True(result.HasWinner);
        Assert.Equal(Mark.X, result.Winner);
        Assert.Equal(new[] { 0, 4, 8 }, result.WinningCells);
    }

    [Fact]
    public void Evaluate_AntiDiagonalWin_IsDetected()
    {
        var board = Board.ReplayMoves(new[]
        {
            new Move(1, Mark.O, 2),
            new Move(2, Mark.X, 0),
            new Move(3, Mark.O, 4),
            new Move(4, Mark.X, 1),
            new Move(5, Mark.O, 6) // completes anti-diagonal: 2,4,6
        });

        var result = _sut.Evaluate(board);

        Assert.True(result.HasWinner);
        Assert.Equal(Mark.O, result.Winner);
        Assert.Equal(new[] { 2, 4, 6 }, result.WinningCells);
    }

    [Fact]
    public void Evaluate_NoWinningLine_ReturnsNoWinner()
    {
        var board = Board.ReplayMoves(new[]
        {
            new Move(1, Mark.X, 0),
            new Move(2, Mark.O, 1)
        });

        var result = _sut.Evaluate(board);

        Assert.False(result.HasWinner);
        Assert.Null(result.WinningCells);
    }
}
