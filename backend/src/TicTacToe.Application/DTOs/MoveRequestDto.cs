namespace TicTacToe.Application.DTOs;

public record MoveRequestDto(string Player, int? CellIndex, int? Row, int? Column);
