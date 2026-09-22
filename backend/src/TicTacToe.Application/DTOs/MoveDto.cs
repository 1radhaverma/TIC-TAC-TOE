namespace TicTacToe.Application.DTOs;


public record MoveDto(int MoveNumber, string Player, int Row, int Column, int CellIndex);
