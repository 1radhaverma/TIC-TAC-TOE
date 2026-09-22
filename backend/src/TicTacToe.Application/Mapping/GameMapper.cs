using TicTacToe.Application.DTOs;
using TicTacToe.Domain.Entities;
using TicTacToe.Domain.Enums;

namespace TicTacToe.Application.Mapping;


public static class GameMapper
{
    public static GameStateDto ToGameStateDto(Game game, ScoreboardDto scoreboard)
    {
        return new GameStateDto(
            Id: game.Id,
            Board: game.Board.Cells.Select(MarkToBoardCell).ToList(),
            CurrentPlayer: MarkToString(game.CurrentPlayer),
            Mode: GameModeToString(game.Mode),
            Status: GameStatusToString(game.Status),
            Winner: game.Winner.HasValue ? MarkToString(game.Winner.Value) : null,
            WinningCells: game.WinningCells,
            MoveHistory: game.MoveHistory.Select(ToMoveDto).ToList(),
            CanUndo: game.MoveHistory.Count > 0 && game.Status == GameStatus.InProgress,
            Scoreboard: scoreboard);
    }

    public static ScoreboardDto ToScoreboardDto(Scoreboard scoreboard) =>
        new(scoreboard.XWins, scoreboard.OWins, scoreboard.Draws);

    private static MoveDto ToMoveDto(Move move) =>
        new(move.MoveNumber, MarkToString(move.Player), move.Row + 1, move.Column + 1, move.CellIndex);

    /// <summary>An empty cell is serialised as JSON null rather than the string "Empty".</summary>
    private static string? MarkToBoardCell(Mark mark) => mark == Mark.Empty ? null : MarkToString(mark);

    public static string MarkToString(Mark mark) => mark switch
    {
        Mark.X => "X",
        Mark.O => "O",
        _ => throw new ArgumentOutOfRangeException(nameof(mark), mark, "Empty is not a valid player.")
    };

    public static Mark StringToMark(string value) => value.Trim().ToUpperInvariant() switch
    {
        "X" => Mark.X,
        "O" => Mark.O,
        _ => throw new ArgumentException($"'{value}' is not a valid player - expected \"X\" or \"O\".", nameof(value))
    };

    public static string GameModeToString(GameMode mode) => mode switch
    {
        GameMode.TwoPlayer => "TwoPlayer",
        GameMode.VsComputer => "VsComputer",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    public static GameMode StringToGameMode(string value) => value.Trim() switch
    {
        "TwoPlayer" => GameMode.TwoPlayer,
        "VsComputer" => GameMode.VsComputer,
        _ => throw new ArgumentException($"'{value}' is not a valid mode - expected \"TwoPlayer\" or \"VsComputer\".", nameof(value))
    };

    private static string GameStatusToString(GameStatus status) => status switch
    {
        GameStatus.InProgress => "InProgress",
        GameStatus.Won => "Won",
        GameStatus.Draw => "Draw",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };
}
