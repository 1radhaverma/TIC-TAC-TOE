using TicTacToe.Domain.Enums;

namespace TicTacToe.Domain.Entities;


public record Move(int MoveNumber, Mark Player, int CellIndex)
{
    /// <summary>0-based row, derived from the cell index - never stored twice.</summary>
    public int Row => CellIndex / Board.Size;

    /// <summary>0-based column, derived from the cell index - never stored twice.</summary>
    public int Column => CellIndex % Board.Size;
}
