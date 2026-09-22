namespace TicTacToe.Domain.Enums;


public enum GameMode
{
    /// <summary>Two human players alternate turns as X and O.</summary>
    TwoPlayer = 0,

    /// <summary>A human plays X; the computer automatically plays O.</summary>
    VsComputer = 1
}
