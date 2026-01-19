using System.Collections.Concurrent;

namespace NppApi.Middleware;

// TODO: Implementiraj rate limiting za MovePaddle pozive
// Ograniči na npr. 60 poziva u sekundi po korisniku
public class RateLimitingMiddleware
{
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _requestLog = new();
    
    // Implementacija...
}
