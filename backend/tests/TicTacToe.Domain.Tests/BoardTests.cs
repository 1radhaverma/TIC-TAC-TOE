using TicTacToe.Domain.Entities;
using TicTacToe.Domain.Enums;
using Xunit;

namespace TicTacToe.Domain.Tests;

public class BoardTests
{
    [Fact]
    public void NewBoard_AllCellsAreEmpty()
    {
        var board = new Board();

        Assert.All(Enumerable.Range(0, Board.CellCount), i => Assert.True(board.IsCellEmpty(i)));
        Assert.False(board.IsFull());
    }

    [Fact]
    public void PlaceMark_OnEmptyCell_StoresTheMark()
    {
        var board = new Board();

        board.PlaceMark(4, Mark.X);

        Assert.Equal(Mark.X, board.GetCell(4));
        Assert.False(board.IsCellEmpty(4));
    }

    [Fact]
    public void PlaceMark_OnOccupiedCell_Throws()
    {
        var board = new Board();
        board.PlaceMark(0, Mark.X);

        Assert.Throws<InvalidOperationException>(() => board.PlaceMark(0, Mark.O));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    public void PlaceMark_OutOfRangeIndex_Throws(int index)
    {
        var board = new Board();

        Assert.Throws<ArgumentOutOfRangeException>(() => board.PlaceMark(index, Mark.X));
    }

    [Fact]
    public void IsFull_ReturnsTrue_OnlyWhenEveryCellIsPlayed()
    {
        var board = new Board();
        for (var i = 0; i < Board.CellCount - 1; i++)
        {
            board.PlaceMark(i, i % 2 == 0 ? Mark.X : Mark.O);
        }

        Assert.False(board.IsFull());

        board.PlaceMark(Board.CellCount - 1, Mark.X);

        Assert.True(board.IsFull());
    }

    [Fact]
    public void ReplayMoves_RebuildsExactlyTheGivenMoves()
    {
        var moves = new[]
        {
            new Move(1, Mark.X, 0),
            new Move(2, Mark.O, 4),
            new Move(3, Mark.X, 8)
        };

        var board = Board.ReplayMoves(moves);

        Assert.Equal(Mark.X, board.GetCell(0));
        Assert.Equal(Mark.O, board.GetCell(4));
        Assert.Equal(Mark.X, board.GetCell(8));
        Assert.True(board.IsCellEmpty(1));
    }

    [Fact]
    public void Clone_ProducesAnIndependentCopy()
    {
        var board = new Board();
        board.PlaceMark(0, Mark.X);

        var clone = board.Clone();
        clone.PlaceMark(1, Mark.O);

        Assert.True(board.IsCellEmpty(1)); // original board is untouched by the clone's change
        Assert.False(clone.IsCellEmpty(1));
    }
}
