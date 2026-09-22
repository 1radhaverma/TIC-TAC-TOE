using TicTacToe.Application.DTOs;
using TicTacToe.Application.Interfaces;
using TicTacToe.Application.Mapping;
using TicTacToe.Domain.Entities;
using TicTacToe.Domain.Enums;
using TicTacToe.Domain.Exceptions;
using TicTacToe.Domain.Rules;
using TicTacToe.Domain.Strategies;

namespace TicTacToe.Application.Services;

/// <inheritdoc cref="IGameService"/>
public class GameService : IGameService
{
    // The computer always plays O; the human always plays X. This is fixed by
    // the spec ("Human player is X, Computer player is O"), so it is a constant
    // here rather than something callers can configure.
    private const Mark ComputerMark = Mark.O;
    private const Mark HumanMark = Mark.X;

    private readonly IGameRepository _gameRepository;
    private readonly IScoreboardService _scoreboardService;
    private readonly IWinChecker _winChecker;
    private readonly IComputerPlayerStrategy _computerStrategy;

    public GameService(
        IGameRepository gameRepository,
        IScoreboardService scoreboardService,
        IWinChecker winChecker,
        IComputerPlayerStrategy computerStrategy)
    {
        _gameRepository = gameRepository;
        _scoreboardService = scoreboardService;
        _winChecker = winChecker;
        _computerStrategy = computerStrategy;
    }

    public GameStateDto CreateGame(string mode)
    {
        var gameMode = GameMapper.StringToGameMode(mode);
        var game = new Game(Guid.NewGuid(), gameMode);
        _gameRepository.Save(game);
        return ToDto(game);
    }

    public GameStateDto GetGame(Guid gameId) => ToDto(GetGameOrThrow(gameId));

    public GameStateDto SubmitMove(Guid gameId, MoveRequestDto request)
    {
        var game = GetGameOrThrow(gameId);
        var player = GameMapper.StringToMark(request.Player);
        var cellIndex = ResolveCellIndex(request);

        // 1. Apply the human's move. Game.ApplyMove throws InvalidMoveException
        //    for every rule violation the spec lists (wrong turn, occupied cell,
        //    out of bounds, game already finished) - GameService does not
        //    duplicate any of that validation, it just lets the exception
        //    propagate to the API layer (see ExceptionHandlingMiddleware).
        game.ApplyMove(player, cellIndex, _winChecker);
        RecordResultIfJustCompleted(game);

        // 2. In Computer Mode, if the human's move did not already end the game,
        //    the computer immediately plays its reply as part of the same request -
        //    the frontend never has to poll or make a second call for this.
        if (game.Mode == GameMode.VsComputer && game.Status == GameStatus.InProgress)
        {
            var computerCellIndex = _computerStrategy.ChooseMove(game.Board, ComputerMark, HumanMark);
            game.ApplyMove(ComputerMark, computerCellIndex, _winChecker);
            RecordResultIfJustCompleted(game);
        }

        _gameRepository.Save(game);
        return ToDto(game);
    }

    public GameStateDto UndoMove(Guid gameId)
    {
        var game = GetGameOrThrow(gameId);
        game.Undo(_winChecker);
        _gameRepository.Save(game);
        return ToDto(game);
    }

    public GameStateDto ResetGame(Guid gameId)
    {
        var game = GetGameOrThrow(gameId);
        game.Reset();
        _gameRepository.Save(game);
        return ToDto(game);
    }

    private Game GetGameOrThrow(Guid gameId) => _gameRepository.FindById(gameId) ?? throw new GameNotFoundException(gameId);

    /// <summary>
    /// Accepts either a flat cell index or a row/column pair from the request and
    /// resolves them to the single 0-8 index the domain layer works with. Doing
    /// this translation here - rather than in the controller - keeps the
    /// controller a thin HTTP adapter and keeps this "which shape did the client
    /// use" concern out of the Game aggregate, which should not need to know the
    /// wire format at all.
    /// </summary>
    private static int ResolveCellIndex(MoveRequestDto request)
    {
        if (request.CellIndex.HasValue)
        {
            return request.CellIndex.Value;
        }

        if (request.Row.HasValue && request.Column.HasValue)
        {
            return (request.Row.Value * Board.Size) + request.Column.Value;
        }

        throw new InvalidMoveException("A move request must include either 'cellIndex' or both 'row' and 'column'.");
    }

    /// <summary>
    /// Updates the scoreboard exactly once per completed game, right when a game
    /// transitions into Won or Draw. <see cref="Game.ScoreRecorded"/> is what
    /// prevents a second call here (e.g. after the computer's reply, immediately
    /// following the human's move in the same request) from double-counting.
    /// </summary>
    private void RecordResultIfJustCompleted(Game game)
    {
        if (game.Status == GameStatus.InProgress || game.ScoreRecorded)
        {
            return;
        }

        _scoreboardService.RecordResult(game.Status, game.Winner);
        game.MarkScoreRecorded();
    }

    private GameStateDto ToDto(Game game) => GameMapper.ToGameStateDto(game, _scoreboardService.GetScoreboard());
}
