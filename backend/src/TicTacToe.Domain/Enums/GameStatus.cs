namespace TicTacToe.Domain.Enums;

/// <summary>The lifecycle status of a single game session.</summary>
public enum GameStatus
{
    /// <summary>The game has empty cells left and no winner yet.</summary>
    InProgress = 0,

    /// <summary>A player has completed a row, column or diagonal.</summary>
    Won = 1,

    /// <summary>All 9 cells are filled and nobody won.</summary>
    Draw = 2
}
