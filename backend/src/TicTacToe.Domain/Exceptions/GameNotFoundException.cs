namespace TicTacToe.Domain.Exceptions;

/// <summary>Thrown when a requested game id does not exist in the repository. Maps to HTTP 404.</summary>
public class GameNotFoundException : Exception
{
    public GameNotFoundException(Guid gameId) : base($"Game '{gameId}' was not found.")
    {
    }
}
