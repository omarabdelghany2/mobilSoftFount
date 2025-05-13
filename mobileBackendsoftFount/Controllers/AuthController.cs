using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using BCrypt.Net;


namespace mobileBackendsoftFount.Controllers;

[Route("api/auth")]
[ApiController]
    
// [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // 🔹 Register User (Allow specifying Role: "Admin" or "User")
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (_context.Users.Any(u => u.Email == request.Email))
            return BadRequest("User already exists.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role  // Role should be "Admin" or "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("User registered successfully.");
    }

    // 🔹 Login User (Only Requires Email & Password)
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        var user = _context.Users.SingleOrDefault(u => u.Email == loginRequest.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        var token = GenerateJwtToken(user);

        return Ok(new 
        { 
            Token = token, 
            Role = user.Role, 
            Email = user.Email 
        });
    }


    // 🔹 Generate JWT Token
    // private string GenerateJwtToken(User user)
    // {
    //     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
    //     var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    //     var claims = new[]
    //     {
    //         new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    //         new Claim(JwtRegisteredClaimNames.Email, user.Email),
    //         new Claim(ClaimTypes.Role, user.Role),
    //         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    //     };

    //     var token = new JwtSecurityToken(
    //         issuer: _configuration["Jwt:Issuer"],
    //         audience: _configuration["Jwt:Audience"],
    //         claims: claims,
    //         expires: DateTime.UtcNow.AddHours(1),
    //         signingCredentials: creds
    //     );

    //     return new JwtSecurityTokenHandler().WriteToken(token);
    // }


    private string GenerateJwtToken(User user)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("role", user.Role), // Explicitly setting "Role" as the claim type
        new Claim("name",user.Name),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

}

// 🔹 Request Models
public class RegisterRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }  // "Admin" or "User"
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
