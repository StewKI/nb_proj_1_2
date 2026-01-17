using Microsoft.AspNetCore.Mvc;
using NppCore.Models;
using NppCore.Services.Features.Auth;


namespace NppApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
   
    public AuthController(IAuthService authService)
    {
        _authService = authService;
       
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        var (player, token) = await _authService.RegisterAsync(request.Username, request.Email, request.Password);
        
        
        return Ok(new RegisterResponse(player.PlayerId, player.Username, player.Email,token));
    }

    [HttpPost("login")]
    public async Task<ActionResult<RegisterResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);
        if (result == null)
            return Unauthorized("Invalid email or password");

        var (player, token) = result.Value;
        return Ok(new LoginResponse(player.PlayerId, player.Username, player.Email, token));
    }
}
