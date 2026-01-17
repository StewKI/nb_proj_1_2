using NppCore.Models;
using NppCore.Services.Persistence.Cassandra;

namespace NppCore.Services.Features.Match;

public class PlayerMatchesService : IPlayerMatchesService
{
    private readonly ICassandraService _cassandra;

    public PlayerMatchesService(ICassandraService cassandra)
    {
        _cassandra = cassandra;
    }

    public async Task<PlayerMatches> CreateAsync(Guid playerId, Guid opponentId, string year, DateTimeOffset mathc_time, string opponentUsername, string score, string result)
    {
        var playerMatches = new PlayerMatches
        {
            PlayerId = playerId,
            OpponentId = opponentId,
            Year = year,
            Result = result,
            Score = score,
            Match_time = mathc_time,
            OpponentUsername = opponentUsername,
        };

        await _cassandra.ExecuteAsync(
            "INSERT INTO player_matches (player_id,year,match_time,opponent_id,opponent_username,result,score) VALUES (?, ?, ?, ?, ?,?,?)",
            playerMatches.PlayerId,
            playerMatches.Year,
            playerMatches.Match_time,
            playerMatches.OpponentId,
            playerMatches.OpponentUsername,
            playerMatches.Result,
            playerMatches.Score
            
        );

        return playerMatches;
    }

    public async Task<IEnumerable<PlayerMatches>> GetByYearAsync(string year, Guid playerId)
    {
        var query = "SELECT * FROM player_matches WHERE year = ? AND player_id = ?";
        return await _cassandra.QueryAsync<PlayerMatches>(query, year, playerId);
    }

}