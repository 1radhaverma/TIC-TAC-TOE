namespace TicTacToe.Application.DTOs;

/// <summary>Wire shape returned by GET /api/scoreboard and embedded in every game state response.</summary>
public record ScoreboardDto(int XWins, int OWins, int Draws);
