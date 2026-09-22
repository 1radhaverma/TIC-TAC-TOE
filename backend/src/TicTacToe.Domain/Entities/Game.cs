using TicTacToe.Domain.Enums;
using TicTacToe.Domain.Exceptions;
using TicTacToe.Domain.Rules;

namespace TicTacToe.Domain.Entities;


public class Game
{
    private readonly List<Move> _moveHistory = new();

    public Guid Id { get; }
    public GameMode Mode { get; }
    public Board Board { get; private set; }
    public Mark CurrentPlayer { get; private set; }
    public GameStatus Status { get; private set; }
    public Mark? Winner { get; private set; }
    public IReadOnlyList<int>? WinningCells { get; private set; }
    public IReadOnlyList<Move> MoveHistory => _moveHistory;

    /// <summary>
    /// True once this game's outcome has already been added to the scoreboard.
    /// Exists purely so <c>GameService</c> can guarantee "the scoreboard updates
    /// only once per completed game" without needing its own bookkeeping -
    /// the game itself is the natural place to know whether it has already
    /// reported its result.
    /// </summary>
    public bool ScoreRecorded { get; private set; }

    public Game(Guid id, GameMode mode)
    {
        Id = id;
        Mode = mode;
        Board = new Board();
        CurrentPlayer = Mark.X; // X always starts a fresh game.
        Status = GameStatus.InProgress;
    }

    /// <summary>
    /// Validates and applies one move, then re-evaluates the game's status.
    /// </summary>
    /// <param name="player">The player attempting the move.</param>
    /// <param name="cellIndex">0-8 flat index of the target cell.</param>
    /// <param name="winChecker">Injected so Game never hard-codes a specific win rule.</param>
    /// <exception cref="InvalidMoveException">
    /// The move breaks a rule: game already finished, cell out of range,
    /// wrong player's turn, or the cell is already occupied.
    /// </exception>
    public void ApplyMove(Mark player, int cellIndex, IWinChecker winChecker)
    {
        if (Status != GameStatus.InProgress)
        {
            throw new InvalidMoveException("This game has already finished; no further moves are allowed.");
        }

        if (!Board.IsWithinBounds(cellIndex))
        {
            throw new InvalidMoveException($"Cell index {cellIndex} is outside the board.");
        }

        if (player != CurrentPlayer)
        {
            throw new InvalidMoveException($"It is {CurrentPlayer}'s turn, not {player}'s.");
        }

        if (!Board.IsCellEmpty(cellIndex))
        {
            throw new InvalidMoveException($"Cell {cellIndex} is already occupied.");
        }

        // The board itself refuses to overwrite an occupied cell, but we already
        // checked that above so the failure reason reported to the caller is precise.
        Board.PlaceMark(cellIndex, player);
        _moveHistory.Add(new Move(_moveHistory.Count + 1, player, cellIndex));

        RefreshStatusAfterMove(winChecker, movingPlayer: player);
    }

    /// <summary>
    /// Removes the most recent move(s) and rebuilds the board/status from what remains.
    ///
    /// Two Player Mode removes exactly the last move. Computer Mode removes the
    /// computer's last move *and* the human move that came before it, as one
    /// pair - unless the computer has not replied yet (the game ended on the
    /// human's move), in which case only that one human move is removed.
    /// </summary>
    /// <exception cref="InvalidGameOperationException">
    /// There is nothing to undo, or the game has already finished (see the
    /// "Disable Undo After Completion" design decision in the README).
    /// </exception>
    public void Undo(IWinChecker winChecker)
    {
        if (_moveHistory.Count == 0)
        {
            throw new InvalidGameOperationException("There are no moves to undo.");
        }

        if (Status != GameStatus.InProgress)
        {
            throw new InvalidGameOperationException(
                "Undo is disabled once a game has finished (see README: Disable Undo After Completion). Reset the game to play again.");
        }

        var movesToRemove = DetermineUndoCount();
        _moveHistory.RemoveRange(_moveHistory.Count - movesToRemove, movesToRemove);

        // Rebuild rather than reverse: replaying the surviving moves onto a fresh
        // board is simpler and safer than trying to "undo" a win/draw calculation.
        Board = Board.ReplayMoves(_moveHistory);
        RefreshStatusFromScratch(winChecker);
    }

    /// <summary>
    /// Starts a fresh game *session* using the same Id and Mode: empty board, empty
    /// history, X to move, no winner. The scoreboard is untouched - Reset Game and
    /// Reset Scoreboard are deliberately two different operations (see Scoreboard).
    /// </summary>
    public void Reset()
    {
        Board = new Board();
        _moveHistory.Clear();
        CurrentPlayer = Mark.X;
        Status = GameStatus.InProgress;
        Winner = null;
        WinningCells = null;
        ScoreRecorded = false;
    }

    /// <summary>Called by the application layer once this game's result has been added to the scoreboard.</summary>
    public void MarkScoreRecorded() => ScoreRecorded = true;

    /// <summary>Flips X to O or O to X. The only place "who plays next" is computed.</summary>
    public static Mark Opponent(Mark mark) => mark == Mark.X ? Mark.O : Mark.X;

    private int DetermineUndoCount()
    {
        if (Mode != GameMode.VsComputer)
        {
            return 1;
        }

        // In Computer Mode the human is always X and the computer is always O.
        // If the last recorded move was the computer's (O), remove it together
        // with the human move before it. If the last move was the human's (X),
        // the computer has not replied yet (most likely the human just won or
        // drew the game), so only that single move is removed.
        var lastMove = _moveHistory[^1];
        return lastMove.Player == Mark.O && _moveHistory.Count >= 2 ? 2 : 1;
    }

    private void RefreshStatusAfterMove(IWinChecker winChecker, Mark movingPlayer)
    {
        var result = winChecker.Evaluate(Board);
        if (result.HasWinner)
        {
            Status = GameStatus.Won;
            Winner = result.Winner;
            WinningCells = result.WinningCells;
            return;
        }

        if (Board.IsFull())
        {
            Status = GameStatus.Draw;
            return;
        }

        // Game continues: only now does the turn actually pass to the other player.
        Status = GameStatus.InProgress;
        CurrentPlayer = Opponent(movingPlayer);
    }

    private void RefreshStatusFromScratch(IWinChecker winChecker)
    {
        var result = winChecker.Evaluate(Board);
        if (result.HasWinner)
        {
            Status = GameStatus.Won;
            Winner = result.Winner;
            WinningCells = result.WinningCells;
        }
        else if (Board.IsFull())
        {
            Status = GameStatus.Draw;
            Winner = null;
            WinningCells = null;
        }
        else
        {
            Status = GameStatus.InProgress;
            Winner = null;
            WinningCells = null;
        }

        CurrentPlayer = _moveHistory.Count == 0 ? Mark.X : Opponent(_moveHistory[^1].Player);
    }
}
