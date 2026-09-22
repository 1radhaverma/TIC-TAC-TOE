using Microsoft.AspNetCore.Mvc;
using TicTacToe.Application.DTOs;
using TicTacToe.Application.Interfaces;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    // Depends on the IGameService abstraction, not the concrete GameService class -
    // ASP.NET Core's built-in DI container supplies the real implementation at
    // startup (see Program.cs), and a test can supply a fake one instead.
    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    /// <summary>POST /api/games - creates a new game session.</summary>
    [HttpPost]
    public ActionResult<GameStateDto> CreateGame([FromBody] CreateGameRequestDto request)
    {
        var game = _gameService.CreateGame(request.Mode);
        return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
    }

    /// <summary>GET /api/games/{id} - returns the current state of a game.</summary>
    [HttpGet("{id:guid}")]
    public ActionResult<GameStateDto> GetGame(Guid id) => Ok(_gameService.GetGame(id));

    /// <summary>POST /api/games/{id}/moves - submits one player move (and, in Computer Mode, triggers the computer's reply).</summary>
    [HttpPost("{id:guid}/moves")]
    public ActionResult<GameStateDto> SubmitMove(Guid id, [FromBody] MoveRequestDto request) =>
        Ok(_gameService.SubmitMove(id, request));

    /// <summary>POST /api/games/{id}/undo - undoes the last move (or move pair in Computer Mode).</summary>
    [HttpPost("{id:guid}/undo")]
    public ActionResult<GameStateDto> UndoMove(Guid id) => Ok(_gameService.UndoMove(id));

    /// <summary>POST /api/games/{id}/reset - resets the board/history/status for this game; the scoreboard is untouched.</summary>
    [HttpPost("{id:guid}/reset")]
    public ActionResult<GameStateDto> ResetGame(Guid id) => Ok(_gameService.ResetGame(id));
}
