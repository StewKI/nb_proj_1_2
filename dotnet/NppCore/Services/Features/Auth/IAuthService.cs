using NppCore.Models;

namespace NppCore.Services.Features.Auth;

public interface IAuthService
{
    Task<(PlayerEntity Player, string Token)> RegisterAsync(string username, string email, string password);
    Task<(PlayerEntity Player, string Token)?> LoginAsync(string email, string password);
}
