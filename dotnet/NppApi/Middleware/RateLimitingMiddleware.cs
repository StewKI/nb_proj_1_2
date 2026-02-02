using System.Collections.Concurrent;

namespace NppApi.Middleware;

// Rate limiting middleware for high-frequency endpoints like MovePaddle
// Limits requests to 60 calls per second per user
public class RateLimitingMiddleware
{
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _requestLog = new();
}
