using TicTacToe.Domain.Entities;
using TicTacToe.Domain.Enums;
using TicTacToe.Domain.Exceptions;
using TicTacToe.Domain.Rules;
using Xunit;

namespace TicTacToe.Domain.Tests;

public class GameTests
{
    private readonly IWinChecker _winChecker = new WinChecker();

    private Game NewGame(GameMode mode = GameMode.TwoPlayer) => new(Guid.NewGuid(), mode);

    [Fact]
    public void ApplyMove_ValidMove_IsRecordedAndSwitchesTurn()
    {
        var game = NewGame();

        game.ApplyMove(Mark.X, 0, _winChecker);

        Assert.Equal(Mark.X, game.Board.GetCell(0));
        Assert.Single(game.MoveHistory);
        Assert.Equal(Mark.O, game.CurrentPlayer); // turn switches after a valid move
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void ApplyMove_WrongPlayersTurn_ThrowsAndDoesNotChangeTheTurn()
    {
        var game = NewGame(); // X moves first

        Assert.Throws<InvalidMoveException>(() => game.ApplyMove(Mark.O, 0, _winChecker));
        Assert.Equal(Mark.X, game.CurrentPlayer); // invalid moves must not change whose turn it is
        Assert.Empty(game.MoveHistory);
    }

    [Fact]
    public void ApplyMove_OccupiedCell_Throws()
    {
        var game = NewGame();
        game.ApplyMove(Mark.X, 0, _winChecker);

        Assert.Throws<InvalidMoveException>(() => game.ApplyMove(Mark.O, 0, _winChecker));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    public void ApplyMove_CellOutsideBoard_Throws(int cellIndex)
    {
        var game = NewGame();

        Assert.Throws<InvalidMoveException>(() => game.ApplyMove(Mark.X, cellIndex, _winChecker));
    }

    [Fact]
    public void ApplyMove_AfterGameHasFinished_Throws()
    {
        var game = NewGame();
        PlayRowWinForX(game); // X wins on the top row

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Throws<InvalidMoveException>(() => game.ApplyMove(Mark.O, 8, _winChecker));
    }

    [Fact]
    public void ApplyMove_CompletingARow_EndsTheGameAsWon()
    {
        var game = NewGame();
        PlayRowWinForX(game);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Mark.X, game.Winner);
        Assert.Equal(new[] { 0, 1, 2 }, game.WinningCells);
    }

    [Fact]
    public void ApplyMove_BoardFullWithNoWinner_EndsTheGameAsDraw()
    {
        var game = NewGame();

        // X | O | X
        // X | O | O
        // O | X | X
        var moves = new (Mark Player, int Cell)[]
        {
            (Mark.X, 0), (Mark.O, 1), (Mark.X, 2),
            (Mark.O, 4), (Mark.X, 3), (Mark.O, 5),
            (Mark.X, 7), (Mark.O, 6), (Mark.X, 8)
        };

        foreach (var (player, cell) in moves)
        {
            game.ApplyMove(player, cell, _winChecker);
        }

        Assert.Equal(GameStatus.Draw, game.Status);
        Assert.Null(game.Winner);
    }

    [Fact]
    public void Reset_ClearsBoardHistoryAndStatus_ButKeepsModeAndId()
    {
        var game = NewGame(GameMode.VsComputer);
        var originalId = game.Id;
        PlayRowWinForX(game);

        game.Reset();

        Assert.Equal(originalId, game.Id);
        Assert.Equal(GameMode.VsComputer, game.Mode);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(Mark.X, game.CurrentPlayer);
        Assert.Empty(game.MoveHistory);
        Assert.Null(game.Winner);
        Assert.Null(game.WinningCells);
        Assert.False(game.ScoreRecorded);
    }

    [Fact]
    public void Undo_WithNoMoves_Throws()
    {
        var game = NewGame();

        Assert.Throws<InvalidGameOperationException>(() => game.Undo(_winChecker));
    }

    [Fact]
    public void Undo_AfterGameCompleted_Throws()
    {
        var game = NewGame();
        PlayRowWinForX(game);

        Assert.Throws<InvalidGameOperationException>(() => game.Undo(_winChecker));
    }

    [Fact]
    public void Undo_TwoPlayerMode_RemovesOnlyTheSingleLastMove()
    {
        var game = NewGame(GameMode.TwoPlayer);
        game.ApplyMove(Mark.X, 0, _winChecker);
        game.ApplyMove(Mark.O, 4, _winChecker);

        game.Undo(_winChecker);

        Assert.Single(game.MoveHistory);
        Assert.Equal(Mark.X, game.Board.GetCell(0));
        Assert.True(game.Board.IsCellEmpty(4));
        Assert.Equal(Mark.O, game.CurrentPlayer); // O's move was undone, so it is O's turn again
    }

    [Fact]
    public void Undo_ComputerMode_AfterComputerReplied_RemovesBothMovesAsAPair()
    {
        var game = NewGame(GameMode.VsComputer);
        game.ApplyMove(Mark.X, 0, _winChecker); // human
        game.ApplyMove(Mark.O, 4, _winChecker); // computer's reply

        game.Undo(_winChecker);

        Assert.Empty(game.MoveHistory);
        Assert.True(game.Board.IsCellEmpty(0));
        Assert.True(game.Board.IsCellEmpty(4));
        Assert.Equal(Mark.X, game.CurrentPlayer); // back to X's turn
    }

    [Fact]
    public void Undo_ComputerMode_WhenComputerHasNotRepliedYet_RemovesOnlyTheHumanMove()
    {
        // The computer always replies immediately after the human within the
        // same GameService.SubmitMove call (see GameService), so the only way
        // a Computer Mode game is left "waiting" on X with an even move count
        // not yet matched by O is if the human's move ended the game right
        // there - which is exactly the case this test drives at, one move
        // before that would happen, so Undo is still allowed (Undo is disabled
        // entirely once the game is Won/Draw - see Undo_AfterGameCompleted_Throws).
        var game = NewGame(GameMode.VsComputer);
        game.ApplyMove(Mark.X, 0, _winChecker);
        game.ApplyMove(Mark.O, 3, _winChecker);
        game.ApplyMove(Mark.X, 1, _winChecker); // computer has not replied to this X move yet

        game.Undo(_winChecker);

        Assert.Equal(2, game.MoveHistory.Count); // only the lone trailing X move was removed
        Assert.True(game.Board.IsCellEmpty(1));
        Assert.Equal(Mark.O, game.Board.GetCell(3));
        Assert.Equal(Mark.X, game.CurrentPlayer);
    }

    /// <summary>Plays the moves X:0, O:3, X:1, O:4, X:2 - X wins on the top row (0,1,2).</summary>
    private void PlayRowWinForX(Game game)
    {
        game.ApplyMove(Mark.X, 0, _winChecker);
        game.ApplyMove(Mark.O, 3, _winChecker);
        game.ApplyMove(Mark.X, 1, _winChecker);
        game.ApplyMove(Mark.O, 4, _winChecker);
        game.ApplyMove(Mark.X, 2, _winChecker);
    }
}
