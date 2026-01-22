using MembershipOperations.Domain.Entities;
using MembershipOperations.Infrastructure.Persistence;
using MembershipOperations.Shared.Dto.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MembershipOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly PasswordHasher<AppUser> _hasher = new();

    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _key;

    public AuthController(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _issuer = config["Jwt:Issuer"]!;
        _audience = config["Jwt:Audience"]!;
        _key = config["Jwt:Key"]!;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var username = request.Username.Trim();

        var exists = await _db.Users.AnyAsync(u => u.Username == username);
        if (exists)
            return Conflict(new ProblemDetails { Title = "Username already exists." });

        // Lock roles down to known values
        var role = request.Role is "Admin" or "Staff" ? request.Role : "Staff";

        var user = new AppUser
        {
            Username = username,
            Role = role,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var username = request.Username.Trim();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        if (user == null)
            return Unauthorized(new ProblemDetails { Title = "Invalid credentials." });

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new ProblemDetails { Title = "Invalid credentials." });

        var expires = DateTime.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("uid", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
                SecurityAlgorithms.HmacSha256
            )
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new AuthResponse
        {
            Token = jwt,
            Username = user.Username,
            Role = user.Role,
            ExpiresUtc = expires
        });
    }
}
