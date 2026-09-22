using TicTacToe.Domain.Entities;

namespace TicTacToe.Domain.Rules;


public interface IWinChecker
{
    /// <summary>Evaluates the board and reports whether a player has won.</summary>
    WinResult Evaluate(Board board);
}
