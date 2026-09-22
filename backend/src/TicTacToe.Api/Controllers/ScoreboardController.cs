using Microsoft.AspNetCore.Mvc;
using TicTacToe.Application.DTOs;
using TicTacToe.Application.Interfaces;

namespace TicTacToe.Api.Controllers;

/// <summary>REST surface for the session scoreboard - separate from <see cref="GamesController"/> because it is a separate concern (Interface Segregation, applied at the controller level too).</summary>
[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController : ControllerBase
{
    private readonly IScoreboardService _scoreboardService;

    public ScoreboardController(IScoreboardService scoreboardService)
    {
        _scoreboardService = scoreboardService;
    }

    /// <summary>GET /api/scoreboard - returns X wins / O wins / draws for the current session.</summary>
    [HttpGet]
    public ActionResult<ScoreboardDto> GetScoreboard() => Ok(_scoreboardService.GetScoreboard());

    /// <summary>POST /api/scoreboard/reset - zeroes every counter. Does not affect any in-progress game.</summary>
    [HttpPost("reset")]
    public ActionResult<ScoreboardDto> ResetScoreboard() => Ok(_scoreboardService.ResetScoreboard());
}
