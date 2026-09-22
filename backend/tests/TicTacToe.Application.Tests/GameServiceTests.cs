using TicTacToe.Application.DTOs;
using TicTacToe.Application.Services;
using TicTacToe.Application.Tests.Fakes;
using TicTacToe.Domain.Exceptions;
using TicTacToe.Domain.Rules;
using TicTacToe.Domain.Strategies;
using Xunit;

namespace TicTacToe.Application.Tests;

public class GameServiceTests
{
    /// <summary>
    /// Builds a real GameService wired up with fakes for storage and the real
    /// (deterministic) domain rule classes for everything else. Only the
    /// storage boundary needs faking here - WinChecker and BasicComputerStrategy
    /// are plain, dependency-free domain logic, so using the real ones keeps
    /// these tests honest about actual game behaviour.
    /// </summary>
    private static (GameService Service, FakeGameRepository Games, FakeScoreboardRepository Scoreboard) CreateService()
    {
        var gameRepository = new FakeGameRepository();
        var scoreboardRepository = new FakeScoreboardRepository();
        var scoreboardService = new ScoreboardService(scoreboardRepository);
        var winChecker = new WinChecker();
        var computerStrategy = new BasicComputerStrategy(winChecker);

        var service = new GameService(gameRepository, scoreboardService, winChecker, computerStrategy);
        return (service, gameRepository, scoreboardRepository);
    }

    [Fact]
    public void CreateGame_ReturnsNewInProgressGame_WithRequestedMode()
    {
        var (service, _, _) = CreateService();

        var dto = service.CreateGame("TwoPlayer");

        Assert.Equal("TwoPlayer", dto.Mode);
        Assert.Equal("InProgress", dto.Status);
        Assert.Equal("X", dto.CurrentPlayer);
        Assert.All(dto.Board, cell => Assert.Null(cell));
        Assert.Empty(dto.MoveHistory);
    }

    [Fact]
    public void GetGame_UnknownId_ThrowsGameNotFoundException()
    {
        var (service, _, _) = CreateService();

        Assert.Throws<GameNotFoundException>(() => service.GetGame(Guid.NewGuid()));
    }

    [Fact]
    public void SubmitMove_ValidMove_TwoPlayerMode_UpdatesBoardAndSwitchesTurn()
    {
        var (service, _, _) = CreateService();
        var game = service.CreateGame("TwoPlayer");

        var updated = service.SubmitMove(game.Id, new MoveRequestDto("X", 0, null, null));

        Assert.Equal("X", updated.Board[0]);
        Assert.Equal("O", updated.CurrentPlayer);
        Assert.Single(updated.MoveHistory);
        Assert.Equal(1, updated.MoveHistory[0].MoveNumber);
        Assert.Equal(1, updated.MoveHistory[0].Row);    // cell 0 -> 1-based Row 1
        Assert.Equal(1, updated.MoveHistory[0].Column); // cell 0 -> 1-based Column 1
    }

    [Fact]
    public void SubmitMove_RowAndColumn_ResolvesToTheSameCellAsIndex()
    {
        var (service, _, _) = CreateService();
        var game = service.CreateGame("TwoPlayer");

        // Row 1, Column 2 (0-based) is cell index 5.
        var updated = service.SubmitMove(game.Id, new MoveRequestDto("X", null, 1, 2));

        Assert.Equal("X", updated.Board[5]);
    }

    [Fact]
    public void SubmitMove_WrongPlayersTurn_ThrowsAndLeavesGameUnchanged()
    {
        var (service, _, _) = CreateService();
        var game = service.CreateGame("TwoPlayer"); // X to move first

        Assert.Throws<InvalidMoveException>(() => service.SubmitMove(game.Id, new MoveRequestDto("O", 0, null, null)));

        var unchanged = service.GetGame(game.Id);
        Assert.Empty(unchanged.MoveHistory);
        Assert.Equal("X", unchanged.CurrentPlayer);
    }

    [Fact]
    public void SubmitMove_VsComputerMode_ComputerReplyIsAppliedAutomatically()
    {
        var (service, _, _) = CreateService();
        var game = service.CreateGame("VsComputer");

        var updated = service.SubmitMove(game.Id, new MoveRequestDto("X", 0, null, null));

        // The human's move (X at 0) and the computer's automatic reply (O at
        // some cell) should both be reflected in a single response.
        Assert.Equal(2, updated.MoveHistory.Count);
        Assert.Equal("X", updated.MoveHistory[0].Player);
        Assert.Equal("O", updated.MoveHistory[1].Player);
        Assert.Equal("X", updated.CurrentPlayer); // turn passed back to the human
    }

    [Fact]
    public void SubmitMove_CompletingTheGame_UpdatesScoreboardExactlyOnce()
    {
        var (service, _, scoreboardRepository) = CreateService();
        var game = service.CreateGame("TwoPlayer");

        // X: 0, O: 3, X: 1, O: 4, X: 2 -> X completes the top row.
        service.SubmitMove(game.Id, new MoveRequestDto("X", 0, null, null));
        service.SubmitMove(game.Id, new MoveRequestDto("O", 3, null, null));
        service.SubmitMove(game.Id, new MoveRequestDto("X", 1, null, null));
        service.SubmitMove(game.Id, new MoveRequestDto("O", 4, null, null));
        var finalState = service.SubmitMove(game.Id, new MoveRequestDto("X", 2, null, null));

        Assert.Equal("Won", finalState.Status);
        Assert.Equal("X", finalState.Winner);
        Assert.Equal(1, scoreboardRepository.Get().XWins);

        // Any further move must be rejected - which also guarantees the
        // scoreboard cannot be double-counted for this game.
        Assert.Throws<InvalidMoveException>(() => service.SubmitMove(game.Id, new MoveRequestDto("O", 5, null, null)));
        Assert.Equal(1, scoreboardRepository.Get().XWins);
    }

    [Fact]
    public void UndoMove_TwoPlayerMode_RemovesOnlyTheLastMove()
    {
        var (service, _, _) = CreateService();
        var game = service.CreateGame("TwoPlayer");
        service.SubmitMove(game.Id, new MoveRequestDto("X", 0, null, null));
        service.SubmitMove(game.Id, new MoveRequestDto("O", 4, null, null));

        var afterUndo = service.UndoMove(game.Id);

        Assert.Single(afterUndo.MoveHistory);
        Assert.Equal("O", afterUndo.CurrentPlayer);
        Assert.Null(afterUndo.Board[4]);
    }

    [Fact]
    public void UndoMove_WithNoMoves_ThrowsInvalidGameOperationException()
    {
        var (service, _, _) = CreateService();
        var game = service.CreateGame("TwoPlayer");

        Assert.Throws<InvalidGameOperationException>(() => service.UndoMove(game.Id));
    }

    [Fact]
    public void ResetGame_ClearsBoardAndHistory_ButScoreboardIsUnaffected()
    {
        var (service, _, scoreboardRepository) = CreateService();
        var game = service.CreateGame("TwoPlayer");
        service.SubmitMove(game.Id, new MoveRequestDto("X", 0, null, null));
        service.SubmitMove(game.Id, new MoveRequestDto("O", 3, null, null));
        service.SubmitMove(game.Id, new MoveRequestDto("X", 1, null, null));
        service.SubmitMove(game.Id, new MoveRequestDto("O", 4, null, null));
        service.SubmitMove(game.Id, new MoveRequestDto("X", 2, null, null)); // X wins -> scoreboard now 1-0-0

        var afterReset = service.ResetGame(game.Id);

        Assert.Equal("InProgress", afterReset.Status);
        Assert.Empty(afterReset.MoveHistory);
        Assert.All(afterReset.Board, cell => Assert.Null(cell));
        Assert.Equal(1, scoreboardRepository.Get().XWins); // Reset Game must not touch the scoreboard
    }
}
