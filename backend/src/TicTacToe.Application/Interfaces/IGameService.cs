using TicTacToe.Application.DTOs;

namespace TicTacToe.Application.Interfaces;

public interface IGameService
{
    /// <summary>Creates a brand-new game session in the given mode and persists it.</summary>
    GameStateDto CreateGame(string mode);

    /// <summary>Returns the current state of an existing game.</summary>
    GameStateDto GetGame(Guid gameId);

    /// <summary>
    /// Applies a human move, then - in Computer Mode, if the game is still in
    /// progress - automatically applies the computer's reply before returning.
    /// </summary>
    GameStateDto SubmitMove(Guid gameId, MoveRequestDto request);

    /// <summary>Undoes the last move (or move pair - see Game.Undo) for the given game.</summary>
    GameStateDto UndoMove(Guid gameId);

    /// <summary>Resets a game session to a fresh board without touching the scoreboard.</summary>
    GameStateDto ResetGame(Guid gameId);
}
