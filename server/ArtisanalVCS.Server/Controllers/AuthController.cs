using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtisanalVCS.Server.Data;
using ArtisanalVCS.Server.DTOs;
using ArtisanalVCS.Server.Models;
using ArtisanalVCS.Server.Services;

namespace ArtisanalVCS.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Username already taken");

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email already taken");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCryptHash(request.Password),
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCryptVerify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid username or password");

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
        });
    }

    private static string BCryptHash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool BCryptVerify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
