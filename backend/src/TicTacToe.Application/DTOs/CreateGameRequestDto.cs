namespace TicTacToe.Application.DTOs;

/// <summary>Request body for POST /api/games. Mode is "TwoPlayer" or "VsComputer".</summary>
public record CreateGameRequestDto(string Mode);
