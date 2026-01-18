using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NppCore.Models;
using NppCore.Services.Features.Match;
using System.Security.Claims;

namespace NppApi.Controllers;


[ApiController]
[Route("api/[controller]")]
public class PlayerMatchesController : ControllerBase
{
    private readonly IPlayerMatchesService _playerMatchesService;

    public PlayerMatchesController(IPlayerMatchesService matchService)
    {
        _playerMatchesService = matchService;
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<ActionResult<PlayerMatchesResponse>> CreateAsync([FromBody] PlayerMatchesRequest request)
    {
        var playerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(playerIdString))
        {
            return Unauthorized("Nismo uspeli da identifikujemo korisnika iz tokena.");
        }

        if (!Guid.TryParse(playerIdString, out Guid playerId))
        {
            return BadRequest("Format ID-a u tokenu nije validan.");
        }

        var pm = await _playerMatchesService.CreateAsync(playerId, request.OpponentId, request.Year, request.Match_time, request.OpponentUsername, request.Score, request.Result);
        return Ok(new PlayerMatchesResponse(pm.PlayerId, pm.OpponentUsername, pm.Result,pm.Score,pm.Match_time));
    }

    [Authorize]
    [HttpGet("{year}")]
    public async Task<ActionResult<IEnumerable<PlayerMatchesResponse>>> GetPlayerMatchesByYearAsync([FromRoute] string year,[FromQuery] int page=1,[FromQuery] int limit=5)
    {
        if (string.IsNullOrEmpty(year) || year.Length != 4)
        {
            return BadRequest("Godina mora biti u formatu YYYY.");
        }

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized("Nismo uspeli da identifikujemo korisnika iz tokena.");
        }

        if (!Guid.TryParse(userIdString, out Guid playerId))
        {
            return Unauthorized("Nevalidan token.");
        }

        var matches= await _playerMatchesService.GetByYearAsync(year,playerId,page,limit);

        var respone= matches.Select(match=>new PlayerMatchesResponse(match.PlayerId, match.OpponentUsername,match.Score, match.Result,match.Match_time));

        return Ok(respone);
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<MatchHistoryResponse>>> GetHistoryAsync([FromQuery] int page=1,[FromQuery] int limit=10)
    {
        if (limit>100) limit=100;
        

        string bucket=DateTimeOffset.UtcNow.ToString("yyyy-MM");
        var matches= await _playerMatchesService.GetHistoryAsync(bucket,page,limit);
        var respone=matches.Select(match=>new MatchHistoryResponse(match.player1Username,match.player2Username,match.Score,match.Result));

        return Ok(respone);
    }

}