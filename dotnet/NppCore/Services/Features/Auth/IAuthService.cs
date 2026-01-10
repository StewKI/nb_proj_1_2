using NppCore.Models;

namespace NppCore.Services.Features.Auth;

public interface IAuthService
{
    Task<PlayerEntity> RegisterAsync(string username, string email, string password);
    Task<PlayerEntity?> LoginAsync(string email, string password);
}
