using TicTacToe.Domain.Enums;

namespace TicTacToe.Domain.Entities;

/// <summary>
/// A 3x3 Tic Tac Toe grid.
///
public class Board
{
    /// <summary>Tic Tac Toe is always played on a 3x3 grid; a 3x3 grid always has 9 cells.</summary>
    public const int Size = 3;
    public const int CellCount = Size * Size;

    private readonly Mark[] _cells;

    public Board()
    {
        _cells = new Mark[CellCount];
    }

    /// <summary>Creates a board from a known set of cells - used when rebuilding a board during Undo.</summary>
    private Board(Mark[] cells)
    {
        _cells = cells;
    }

    /// <summary>Read-only view of the 9 cells, in index order.</summary>
    public IReadOnlyList<Mark> Cells => _cells;

    public Mark GetCell(int index)
    {
        EnsureValidIndex(index);
        return _cells[index];
    }

    public bool IsCellEmpty(int index)
    {
        EnsureValidIndex(index);
        return _cells[index] == Mark.Empty;
    }

    public bool IsWithinBounds(int index) => index >= 0 && index < CellCount;

    /// <summary>True once every cell has been played.</summary>
    public bool IsFull() => _cells.All(c => c != Mark.Empty);

    /// <summary>
    /// Places a mark on a cell. This is the only place a cell's value ever changes,
    /// which is what makes the board's one invariant ("never overwrite a played
    /// cell") easy to guarantee.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Index is not 0-8.</exception>
    /// <exception cref="InvalidOperationException">The cell is already occupied.</exception>
    public void PlaceMark(int index, Mark mark)
    {
        EnsureValidIndex(index);
        if (mark == Mark.Empty)
        {
            throw new ArgumentException("Cannot place an empty mark on the board.", nameof(mark));
        }

        if (_cells[index] != Mark.Empty)
        {
            throw new InvalidOperationException($"Cell {index} is already occupied.");
        }

        _cells[index] = mark;
    }

    /// <summary>
    /// Produces a brand-new board with the given moves replayed onto it, in order.
    /// Used by <see cref="Game.Undo"/>: rather than trying to "un-apply" a move
    /// (which would need to reverse win/draw calculations too), it is simpler and
    /// far less error-prone to rebuild the board from the moves that remain.
    /// </summary>
    public static Board ReplayMoves(IEnumerable<Move> moves)
    {
        var board = new Board();
        foreach (var move in moves)
        {
            board.PlaceMark(move.CellIndex, move.Player);
        }

        return board;
    }

    /// <summary>Deep copy - used so a caller can inspect "what if I played here" without mutating the real board.</summary>
    public Board Clone() => new((Mark[])_cells.Clone());

    private static void EnsureValidIndex(int index)
    {
        if (index < 0 || index >= CellCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Cell index must be between 0 and 8.");
        }
    }
}
