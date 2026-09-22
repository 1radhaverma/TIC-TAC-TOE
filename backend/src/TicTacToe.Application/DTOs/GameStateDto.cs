namespace TicTacToe.Application.DTOs;


public record GameStateDto(
    Guid Id,
    IReadOnlyList<string?> Board,
    string CurrentPlayer,
    string Mode,
    string Status,
    string? Winner,
    IReadOnlyList<int>? WinningCells,
    IReadOnlyList<MoveDto> MoveHistory,
    bool CanUndo,
    ScoreboardDto Scoreboard);
