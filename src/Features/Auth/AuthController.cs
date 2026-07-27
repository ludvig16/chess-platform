using System.Security.Claims;
using ChessPlatform.Features.Auth.Dtos;
using ChessPlatform.Features.Users;
using ChessPlatform.Features.Users.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = ChessPlatform.Features.Auth.Dtos.LoginRequest;

namespace ChessPlatform.Features.Auth;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateUserRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result.IsFailure)
        {
            return Conflict(result.Error);
        }

        return Ok(result.Value!.ToUserResponseDto());
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
    
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new MeResponse
        {
            Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            Username = User.FindFirstValue(ClaimTypes.Name)!,
            Email = User.FindFirstValue(ClaimTypes.Email)!
        });
    }
}