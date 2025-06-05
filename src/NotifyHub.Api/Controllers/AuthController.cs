using Microsoft.AspNetCore.Mvc;
using NotifyHub.Application.Services;

namespace NotifyHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("token")]
    public IActionResult GetToken([FromBody] TokenRequest request)
    {
        var token = _tokenService.GenerateToken(request.UserId, request.Role);
        return Ok(new { token });
    }
}

public class TokenRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
} 