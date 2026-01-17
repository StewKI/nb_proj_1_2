using NppCore.Models;

namespace NppCore.Services.Features.Match;

public interface IPlayerMatchesService
{
    Task<PlayerMatches> CreateAsync(Guid playerId,Guid opponentId, string year, DateTimeOffset mathc_time, string opponentUsername, string score,string result );
    Task<IEnumerable<PlayerMatches>> GetByYearAsync(string year,Guid playerId);
    
}