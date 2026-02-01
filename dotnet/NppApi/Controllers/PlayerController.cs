using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NppCore.Services.Features.Player;

namespace NppApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class PlayerController : ControllerBase
{
    private readonly IPlayerStatsService _statsService;
    private readonly IPlayerService _playerService;

    public PlayerController(IPlayerStatsService statsService, IPlayerService playerService)
    {
        _statsService = statsService;
        _playerService = playerService;
    }

    [HttpGet("me/stats")]
    public async Task<ActionResult<PlayerStatsResponse>> GetMyStats()
    {
        var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(playerIdClaim) || !Guid.TryParse(playerIdClaim, out var playerId))
        {
            return Unauthorized("Invalid player ID");
        }

        var stats = await _statsService.GetStatsAsync(playerId);

        // Return default stats if player hasn't played any games yet
        if (stats == null)
        {
            return Ok(new PlayerStatsResponse(0, 0, 0, "0%"));
        }

        var totalGames = stats.GamesWon + stats.GamesLost;
        var winRate = totalGames > 0
            ? $"{(stats.GamesWon * 100.0 / totalGames):F0}%"
            : "0%";

        return Ok(new PlayerStatsResponse(
            stats.TotalPoints,
            stats.GamesWon,
            stats.GamesLost,
            winRate
        ));
    }
}

public record PlayerStatsResponse(long TotalPoints, long Wins, long Losses, string WinRate);
