using NppCore.Models;
using NppCore.Services.Features.Player;
using NppCore.Services.Persistence.Cassandra;

namespace NppCore.Services.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IPlayerService _playerService;
    private readonly ICassandraService _cassandra;

    public AuthService(IPlayerService playerService, ICassandraService cassandra)
    {
        _playerService = playerService;
        _cassandra = cassandra;
    }

    public async Task<PlayerEntity> RegisterAsync(string username, string email, string password)
    {
        var player = await _playerService.CreateAsync(username, email);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        await _cassandra.ExecuteAsync(
            "INSERT INTO players_by_email (email, password_hash, player_id) VALUES (?, ?, ?)",
            email,
            passwordHash,
            player.PlayerId
        );

        return player;
    }

    public async Task<PlayerEntity?> LoginAsync(string email, string password)
    {
        var playerByEmail = await _cassandra.QueryFirstOrDefaultAsync<PlayerByEmail>(
            "SELECT email, password_hash, player_id FROM players_by_email WHERE email = ?",
            email
        );

        if (playerByEmail == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, playerByEmail.PasswordHash))
            return null;

        return await _playerService.GetByIdAsync(playerByEmail.PlayerId);
    }
}
