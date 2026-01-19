using System.ComponentModel.DataAnnotations;
using NppCore.Constants;

namespace NppCore.Models;

public record RegisterRequest(
    [Required(ErrorMessage = "Username is required")]
    [StringLength(GameConstants.MaxUsernameLength, MinimumLength = GameConstants.MinUsernameLength, 
        ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers and underscores")]
    string Username,
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(GameConstants.MaxPasswordLength, MinimumLength = GameConstants.MinPasswordLength, 
        ErrorMessage = "Password must be at least 6 characters")]
    string Password
);

public record LoginRequest(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,
    
    [Required(ErrorMessage = "Password is required")]
    string Password
);

public record RegisterResponse(Guid PlayerId, string Username, string Email, string Token);

public record LoginResponse(Guid PlayerId, string Username, string Email, string Token);
