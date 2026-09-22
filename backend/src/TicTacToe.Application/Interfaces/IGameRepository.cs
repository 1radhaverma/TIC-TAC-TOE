using TicTacToe.Domain.Entities;

namespace TicTacToe.Application.Interfaces;

public interface IGameRepository
{
    /// <summary>Persists a new game or overwrites an existing one with the same Id.</summary>
    void Save(Game game);

    /// <summary>Returns the game with the given Id, or null if none exists.</summary>
    Game? FindById(Guid id);
}
