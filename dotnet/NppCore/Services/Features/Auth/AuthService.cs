using NppCore.Models;
using NppCore.Services.Features.Player;
using NppCore.Services.Persistence.Cassandra;

namespace NppCore.Services.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IPlayerService _playerService;
    private readonly ICassandraService _cassandra;
    private readonly IJwtService _jwtService;

    public AuthService(
        IPlayerService playerService,
        ICassandraService cassandra,
        IJwtService jwtService)
    {
        _playerService = playerService;
        _cassandra = cassandra;
        _jwtService = jwtService;
    }

    public async Task<(PlayerEntity Player, string Token)> RegisterAsync(
        string username,
        string email,
        string password)
    {
        var existingPlayer = await _playerService.GetByUsernameAsync(username);
        if (existingPlayer != null)
            throw new InvalidOperationException("A user with this username already exists");

        var existingEmail = await _playerService.GetByEmailAsync(email);
        if (existingEmail != null)
            throw new InvalidOperationException("A user with this email already exists");

        var player = await _playerService.CreateAsync(username, email);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        await _cassandra.ExecuteAsync(
            "INSERT INTO players_by_email (email, password_hash, player_id) VALUES (?, ?, ?) ",
            email,
            passwordHash,
            player.PlayerId
        );

        // Index by username for match lookups
        await _cassandra.ExecuteAsync(
            "INSERT INTO players_by_username (username, player_id) VALUES (?, ?)",
            username,
            player.PlayerId
        );

        var token = _jwtService.GenerateToken(
            player.PlayerId,
            player.Username,
            player.Email
        );

        return (player, token);
    }

    public async Task<(PlayerEntity Player, string Token)?> LoginAsync(
        string email,
        string password)
    {
        var playerByEmail = await _cassandra
            .QueryFirstOrDefaultAsync<PlayerByEmail>(
                "SELECT email, password_hash, player_id FROM players_by_email WHERE email = ?",
                email
            );

        if (playerByEmail == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, playerByEmail.PasswordHash))
            return null;

        var player = await _playerService.GetByIdAsync(playerByEmail.PlayerId);
        if (player == null)
            return null;

        var token = _jwtService.GenerateToken(
            player.PlayerId,
            player.Username,
            player.Email
        );

        return (player, token);
    }
}
