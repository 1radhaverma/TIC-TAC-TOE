using TicTacToe.Application.Services;
using TicTacToe.Application.Tests.Fakes;
using TicTacToe.Domain.Enums;
using Xunit;

namespace TicTacToe.Application.Tests;

public class ScoreboardServiceTests
{
    [Fact]
    public void GetScoreboard_NewSession_IsAllZero()
    {
        var service = new ScoreboardService(new FakeScoreboardRepository());

        var dto = service.GetScoreboard();

        Assert.Equal(0, dto.XWins);
        Assert.Equal(0, dto.OWins);
        Assert.Equal(0, dto.Draws);
    }

    [Fact]
    public void RecordResult_IsReflectedImmediately_InGetScoreboard()
    {
        var service = new ScoreboardService(new FakeScoreboardRepository());

        service.RecordResult(GameStatus.Won, Mark.X);
        service.RecordResult(GameStatus.Won, Mark.X);
        service.RecordResult(GameStatus.Won, Mark.O);
        service.RecordResult(GameStatus.Draw, null);

        var dto = service.GetScoreboard();

        Assert.Equal(2, dto.XWins);
        Assert.Equal(1, dto.OWins);
        Assert.Equal(1, dto.Draws);
    }

    [Fact]
    public void ResetScoreboard_ZeroesEveryCounter()
    {
        var service = new ScoreboardService(new FakeScoreboardRepository());
        service.RecordResult(GameStatus.Won, Mark.X);

        var dto = service.ResetScoreboard();

        Assert.Equal(0, dto.XWins);
        Assert.Equal(0, dto.OWins);
        Assert.Equal(0, dto.Draws);
    }
}
