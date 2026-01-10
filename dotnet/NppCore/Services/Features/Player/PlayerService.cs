using NppCore.Models;
using NppCore.Services.Persistence.Cassandra;

namespace NppCore.Services.Features.Player;

public class PlayerService : IPlayerService
{
    private readonly ICassandraService _cassandra;

    public PlayerService(ICassandraService cassandra)
    {
        _cassandra = cassandra;
    }

    public async Task<PlayerEntity> CreateAsync(string username, string email, string? avatarUrl = null)
    {
        var player = new PlayerEntity
        {
            PlayerId = Guid.NewGuid(),
            Username = username,
            Email = email,
            AvatarUrl = avatarUrl,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _cassandra.ExecuteAsync(
            "INSERT INTO players (player_id, username, email, avatar_url, created_at) VALUES (?, ?, ?, ?, ?)",
            player.PlayerId,
            player.Username,
            player.Email,
            player.AvatarUrl!,
            player.CreatedAt
        );

        return player;
    }

    public async Task<PlayerEntity?> GetByIdAsync(Guid playerId)
    {
        return await _cassandra.QueryFirstOrDefaultAsync<PlayerEntity>(
            "SELECT player_id, username, email, avatar_url, created_at FROM players WHERE player_id = ?",
            playerId
        );
    }

    public async Task<PlayerEntity?> GetByEmailAsync(string email)
    {
        var playerByEmail = await _cassandra.QueryFirstOrDefaultAsync<PlayerByEmail>(
            "SELECT email, password_hash, player_id FROM players_by_email WHERE email = ?",
            email
        );

        if (playerByEmail == null)
            return null;

        return await GetByIdAsync(playerByEmail.PlayerId);
    }
}
