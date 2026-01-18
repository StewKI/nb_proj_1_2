using System.Globalization;
using Microsoft.Extensions.Logging;
using NppCore.Models;

namespace NppCore.Services.Persistence.Redis;

public class RedisGameStateRepository : IGameStateRepository
{
    private readonly IRedisService _redis;
    private readonly ILogger<RedisGameStateRepository> _logger;

    private const string GameKeyPrefix = "game:";
    private const string PlayerGameKeyPrefix = "player:game:";
    private const string OpenGamesKey = "games:open";
    private const string PlayingGamesKey = "games:playing";

    public RedisGameStateRepository(IRedisService redis, ILogger<RedisGameStateRepository> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task CreateGameAsync(Game game)
    {
        var key = GameKeyPrefix + game.Id;
        var fields = SerializeGame(game);

        await _redis.HashSetAsync(key, fields);
        await _redis.SetAddAsync(OpenGamesKey, game.Id);

        _logger.LogDebug("Created game {GameId} in Redis", game.Id);
    }

    public async Task<bool> JoinGameAsync(string gameId, Player player2)
    {
        var key = GameKeyPrefix + gameId;

        var fields = new Dictionary<string, string>
        {
            ["player2_id"] = player2.ConnectionId,
            ["player2_name"] = player2.Name,
            ["player2_score"] = "0",
            ["state"] = GameState.Playing.ToString()
        };

        await _redis.HashSetAsync(key, fields);
        await MoveToPlayingGamesAsync(gameId);

        _logger.LogDebug("Player {PlayerId} joined game {GameId} in Redis", player2.ConnectionId, gameId);
        return true;
    }

    public async Task RemoveGameAsync(string gameId)
    {
        var key = GameKeyPrefix + gameId;

        await _redis.KeyDeleteAsync(key);
        await _redis.SetRemoveAsync(OpenGamesKey, gameId);
        await _redis.SetRemoveAsync(PlayingGamesKey, gameId);

        _logger.LogDebug("Removed game {GameId} from Redis", gameId);
    }

    public async Task SetPlayerGameMappingAsync(string connectionId, string gameId)
    {
        var key = PlayerGameKeyPrefix + connectionId;
        await _redis.StringSetAsync(key, gameId);
    }

    public async Task<string?> GetGameIdByPlayerAsync(string connectionId)
    {
        var key = PlayerGameKeyPrefix + connectionId;
        return await _redis.StringGetAsync(key);
    }

    public async Task RemovePlayerMappingAsync(string connectionId)
    {
        var key = PlayerGameKeyPrefix + connectionId;
        await _redis.KeyDeleteAsync(key);
    }

    public async Task SyncGameStateAsync(string gameId, Game game)
    {
        var key = GameKeyPrefix + gameId;
        var fields = new Dictionary<string, string>
        {
            ["ball_x"] = game.Ball.X.ToString(CultureInfo.InvariantCulture),
            ["ball_y"] = game.Ball.Y.ToString(CultureInfo.InvariantCulture),
            ["ball_vx"] = game.Ball.VelocityX.ToString(CultureInfo.InvariantCulture),
            ["ball_vy"] = game.Ball.VelocityY.ToString(CultureInfo.InvariantCulture),
            ["paddle1_y"] = game.Paddle1.Y.ToString(CultureInfo.InvariantCulture),
            ["paddle2_y"] = game.Paddle2.Y.ToString(CultureInfo.InvariantCulture),
            ["player1_score"] = game.Player1?.Score.ToString() ?? "0",
            ["player2_score"] = game.Player2?.Score.ToString() ?? "0",
            ["state"] = game.State.ToString()
        };

        await _redis.HashSetAsync(key, fields);
    }

    public async Task<List<Game>> LoadAllGamesAsync()
    {
        var games = new List<Game>();

        var gameKeys = await _redis.KeysAsync(GameKeyPrefix + "*");

        foreach (var key in gameKeys)
        {
            try
            {
                var fields = await _redis.HashGetAllAsync(key);
                if (fields.Count == 0) continue;

                var game = DeserializeGame(fields);
                if (game != null)
                {
                    games.Add(game);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load game from key {Key}", key);
            }
        }

        _logger.LogInformation("Loaded {Count} games from Redis", games.Count);
        return games;
    }

    public async Task<Dictionary<string, string>> LoadAllPlayerMappingsAsync()
    {
        var mappings = new Dictionary<string, string>();

        var playerKeys = await _redis.KeysAsync(PlayerGameKeyPrefix + "*");

        foreach (var key in playerKeys)
        {
            var connectionId = key.Replace(PlayerGameKeyPrefix, "");
            var gameId = await _redis.StringGetAsync(key);

            if (!string.IsNullOrEmpty(gameId))
            {
                mappings[connectionId] = gameId;
            }
        }

        _logger.LogInformation("Loaded {Count} player mappings from Redis", mappings.Count);
        return mappings;
    }

    public async Task<List<string>> GetOpenGameIdsAsync()
    {
        return await _redis.SetMembersAsync(OpenGamesKey);
    }

    public async Task AddToOpenGamesAsync(string gameId)
    {
        await _redis.SetAddAsync(OpenGamesKey, gameId);
    }

    public async Task RemoveFromOpenGamesAsync(string gameId)
    {
        await _redis.SetRemoveAsync(OpenGamesKey, gameId);
    }

    public async Task MoveToPlayingGamesAsync(string gameId)
    {
        await _redis.SetRemoveAsync(OpenGamesKey, gameId);
        await _redis.SetAddAsync(PlayingGamesKey, gameId);
    }

    private static Dictionary<string, string> SerializeGame(Game game)
    {
        var fields = new Dictionary<string, string>
        {
            ["id"] = game.Id,
            ["state"] = game.State.ToString(),
            ["ball_x"] = game.Ball.X.ToString(CultureInfo.InvariantCulture),
            ["ball_y"] = game.Ball.Y.ToString(CultureInfo.InvariantCulture),
            ["ball_vx"] = game.Ball.VelocityX.ToString(CultureInfo.InvariantCulture),
            ["ball_vy"] = game.Ball.VelocityY.ToString(CultureInfo.InvariantCulture),
            ["paddle1_y"] = game.Paddle1.Y.ToString(CultureInfo.InvariantCulture),
            ["paddle2_y"] = game.Paddle2.Y.ToString(CultureInfo.InvariantCulture)
        };

        if (game.Player1 != null)
        {
            fields["player1_id"] = game.Player1.ConnectionId;
            fields["player1_name"] = game.Player1.Name;
            fields["player1_score"] = game.Player1.Score.ToString();
        }

        if (game.Player2 != null)
        {
            fields["player2_id"] = game.Player2.ConnectionId;
            fields["player2_name"] = game.Player2.Name;
            fields["player2_score"] = game.Player2.Score.ToString();
        }

        return fields;
    }

    private static Game? DeserializeGame(Dictionary<string, string> fields)
    {
        if (!fields.TryGetValue("id", out var id))
            return null;

        var game = new Game { Id = id };

        if (fields.TryGetValue("state", out var stateStr) &&
            Enum.TryParse<GameState>(stateStr, out var state))
        {
            game.State = state;
        }

        if (fields.TryGetValue("ball_x", out var ballX))
            game.Ball.X = double.Parse(ballX, CultureInfo.InvariantCulture);
        if (fields.TryGetValue("ball_y", out var ballY))
            game.Ball.Y = double.Parse(ballY, CultureInfo.InvariantCulture);
        if (fields.TryGetValue("ball_vx", out var ballVx))
            game.Ball.VelocityX = double.Parse(ballVx, CultureInfo.InvariantCulture);
        if (fields.TryGetValue("ball_vy", out var ballVy))
            game.Ball.VelocityY = double.Parse(ballVy, CultureInfo.InvariantCulture);

        if (fields.TryGetValue("paddle1_y", out var paddle1Y))
            game.Paddle1.Y = double.Parse(paddle1Y, CultureInfo.InvariantCulture);
        if (fields.TryGetValue("paddle2_y", out var paddle2Y))
            game.Paddle2.Y = double.Parse(paddle2Y, CultureInfo.InvariantCulture);

        if (fields.TryGetValue("player1_id", out var p1Id) &&
            fields.TryGetValue("player1_name", out var p1Name))
        {
            game.Player1 = new Player
            {
                ConnectionId = p1Id,
                Name = p1Name,
                Score = fields.TryGetValue("player1_score", out var p1Score)
                    ? int.Parse(p1Score)
                    : 0
            };
        }

        if (fields.TryGetValue("player2_id", out var p2Id) &&
            fields.TryGetValue("player2_name", out var p2Name))
        {
            game.Player2 = new Player
            {
                ConnectionId = p2Id,
                Name = p2Name,
                Score = fields.TryGetValue("player2_score", out var p2Score)
                    ? int.Parse(p2Score)
                    : 0
            };
        }

        return game;
    }
}
