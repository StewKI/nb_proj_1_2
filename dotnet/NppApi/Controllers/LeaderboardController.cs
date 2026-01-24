using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NppCore.Models;
using NppCore.Services.Features.Leaderboard;

namespace NppApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    /// GET /api/leaderboard/global?periodType=MONTHLY&periodId=2024-01&limit=10
    [HttpGet("global")]
    public async Task<ActionResult<GlobalLeaderboardResponse>> GetGlobalLeaderboard(
        [FromQuery] string periodType,
        [FromQuery] string periodId,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(periodType) || string.IsNullOrWhiteSpace(periodId))
        {
            return BadRequest("periodType and periodId are required");
        }

        var result = await _leaderboardService.GetGlobalLeaderboardAsync(periodType, periodId, limit);
        return Ok(result);
    }

    /// GET /api/leaderboard/wins?category=most_wins&limit=10
    [HttpGet("wins")]
    public async Task<ActionResult<WinsLeaderboardResponse>> GetWinsLeaderboard(
        [FromQuery] string category = "most_wins",
        [FromQuery] int limit = 10)
    {
        var result = await _leaderboardService.GetWinsLeaderboardAsync(category, limit);
        return Ok(result);
    }

    /// GET /api/leaderboard/streak/{playerId}
    [HttpGet("streak/{playerId:guid}")]
    public async Task<ActionResult<PlayerStreakDto>> GetPlayerStreak(Guid playerId)
    {
        var result = await _leaderboardService.GetPlayerStreakAsync(playerId);

        if (result == null)
            return NotFound(new { message = "Player streak not found" });

        return Ok(result);
    }

    /// GET /api/leaderboard/longest-streak?category=global_all_time&limit=10
    [HttpGet("longest-streak")]
    public async Task<ActionResult<StreakLeaderboardResponse>> GetStreakLeaderboard(
        [FromQuery] string category = "global_all_time",
        [FromQuery] int limit = 10)
    {
        var result = await _leaderboardService.GetStreakLeaderboardAsync(category, limit);
        return Ok(result);
    }

}
