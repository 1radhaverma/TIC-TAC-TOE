using TicTacToe.Application.Interfaces;
using TicTacToe.Domain.Entities;

namespace TicTacToe.Application.Tests.Fakes;


public class FakeGameRepository : IGameRepository
{
    private readonly Dictionary<Guid, Game> _games = new();

    public void Save(Game game) => _games[game.Id] = game;

    public Game? FindById(Guid id) => _games.GetValueOrDefault(id);
}
