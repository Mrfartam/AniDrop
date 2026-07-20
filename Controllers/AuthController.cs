using AniDrop.DBInfrastructure;
using AniDrop.Domain;
using AniDrop.Models;
using AniDrop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AniDrop.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController: ControllerBase
{
    private readonly AniDropDBContext _context;
    private readonly IAuthService _authService;
    public AuthController(AniDropDBContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDTO dto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest("Пользователь с таким никнеймом уже существует");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = _authService.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("Регистрация успешна");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null || !_authService.VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized("Неверное имя пользователя или пароль");

        var token = _authService.GenerateToken(user);

        return Ok(new { Token = token });
    }
}
