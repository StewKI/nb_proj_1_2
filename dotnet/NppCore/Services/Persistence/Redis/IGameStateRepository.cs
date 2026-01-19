using NppCore.Models;

namespace NppCore.Services.Persistence.Redis;

public interface IGameStateRepository
{
    // Lifecycle (immediate writes)
    Task CreateGameAsync(Game game);
    Task<bool> JoinGameAsync(string gameId, Player player2);
    Task RemoveGameAsync(string gameId);

    // Player mapping
    Task SetPlayerGameMappingAsync(string connectionId, string gameId);
    Task<string?> GetGameIdByPlayerAsync(string connectionId);
    Task RemovePlayerMappingAsync(string connectionId);

    // State sync (periodic)
    Task SyncGameStateAsync(string gameId, Game game);

    // Recovery
    Task<List<Game>> LoadAllGamesAsync();
    Task<Dictionary<string, string>> LoadAllPlayerMappingsAsync();

    // Lobby
    Task<List<string>> GetOpenGameIdsAsync();
    Task AddToOpenGamesAsync(string gameId);
    Task RemoveFromOpenGamesAsync(string gameId);
    Task MoveToPlayingGamesAsync(string gameId);

    // Reconnection tokens
    Task<string> CreateReconnectTokenAsync(string gameId, int playerNumber, string playerName, TimeSpan expiry);
    Task<ReconnectSession?> GetReconnectSessionAsync(string token);
    Task RemoveReconnectTokenAsync(string token);
    Task RemoveGameTokensAsync(string gameId);

    // Player connection state
    Task SetPlayerConnectedAsync(string gameId, int playerNumber, bool connected, string? connectionId);
    Task<PlayerConnectionState> GetPlayersConnectionStateAsync(string gameId);
    Task UpdatePlayerConnectionIdAsync(string gameId, int playerNumber, string newConnectionId);

    // Paused games
    Task MoveToPausedGamesAsync(string gameId);
    Task MoveFromPausedToPlayingAsync(string gameId);
    Task<List<string>> GetPausedGameIdsAsync();
}
