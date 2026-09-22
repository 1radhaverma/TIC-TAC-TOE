using TicTacToe.Domain.Entities;
using TicTacToe.Domain.Enums;
using Xunit;

namespace TicTacToe.Domain.Tests;

public class ScoreboardTests
{
    [Fact]
    public void RecordResult_XWin_IncrementsOnlyXWins()
    {
        var scoreboard = new Scoreboard();

        scoreboard.RecordResult(GameStatus.Won, Mark.X);

        Assert.Equal(1, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }

    [Fact]
    public void RecordResult_OWin_IncrementsOnlyOWins()
    {
        var scoreboard = new Scoreboard();

        scoreboard.RecordResult(GameStatus.Won, Mark.O);

        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(1, scoreboard.OWins);
    }

    [Fact]
    public void RecordResult_Draw_IncrementsOnlyDraws()
    {
        var scoreboard = new Scoreboard();

        scoreboard.RecordResult(GameStatus.Draw, null);

        Assert.Equal(1, scoreboard.Draws);
        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
    }

    [Fact]
    public void RecordResult_InProgress_IsIgnored()
    {
        var scoreboard = new Scoreboard();

        scoreboard.RecordResult(GameStatus.InProgress, null);

        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }

    [Fact]
    public void Reset_ZeroesEveryCounter()
    {
        var scoreboard = new Scoreboard();
        scoreboard.RecordResult(GameStatus.Won, Mark.X);
        scoreboard.RecordResult(GameStatus.Won, Mark.O);
        scoreboard.RecordResult(GameStatus.Draw, null);

        scoreboard.Reset();

        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }
}
