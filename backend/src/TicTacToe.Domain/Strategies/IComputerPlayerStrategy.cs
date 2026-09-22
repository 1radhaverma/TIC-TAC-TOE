using TicTacToe.Domain.Entities;
using TicTacToe.Domain.Enums;

namespace TicTacToe.Domain.Strategies;

/// <summary>
/// This is the classic Gang-of-Four <b>Strategy pattern</b>: the algorithm for
/// choosing a move is swapped in as a dependency rather than hard-coded inside
/// <see cref="Game"/> or the application service. Today there is one
/// implementation (<see cref="BasicComputerStrategy"/>), but a harder
/// "MinimaxComputerStrategy" or an "EasyRandomStrategy" could be added later and
/// wired in through dependency injection without changing any calling code
/// (Open/Closed Principle).
/// </summary>
public interface IComputerPlayerStrategy
{
    /// <summary>
    /// Chooses the next cell for <paramref name="computerMark"/> to play.
    /// </summary>
    /// <param name="board">The current board (assumed not full and not already won).</param>
    /// <param name="computerMark">The mark the computer is playing as (always O in this app).</param>
    /// <param name="opponentMark">The human opponent's mark (always X in this app).</param>
    /// <returns>The 0-8 index of the cell the computer chooses to play.</returns>
    int ChooseMove(Board board, Mark computerMark, Mark opponentMark);
}
