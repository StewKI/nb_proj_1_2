namespace NppCore.Models;

public record RegisterRequest(string Username, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record RegisterResponse(Guid PlayerId, string Username, string Email);
