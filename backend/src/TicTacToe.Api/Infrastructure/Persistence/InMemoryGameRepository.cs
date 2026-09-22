using System.Collections.Concurrent;
using TicTacToe.Application.Interfaces;
using TicTacToe.Domain.Entities;

namespace TicTacToe.Api.Infrastructure.Persistence;

public class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();

    public void Save(Game game) => _games[game.Id] = game;

    public Game? FindById(Guid id) => _games.TryGetValue(id, out var game) ? game : null;
}
