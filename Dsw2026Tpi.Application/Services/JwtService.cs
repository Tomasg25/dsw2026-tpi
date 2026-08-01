using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Dsw2026Tpi.Application.Services;

public class JwtService
{
    private readonly IConfiguration _config;
    private readonly ILogger<JwtService> _logger;
    public JwtService(IConfiguration config, ILogger<JwtService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public string GenerateToken(string username, string? role)
    {
        if (_config == null) throw new ArgumentNullException();
        var jwtConfig = _config.GetSection("Jwt");
        var keyText = jwtConfig["Key"] ?? throw new ArgumentNullException("Jwt Key");
        var issuer = jwtConfig["Issuer"] ?? throw new ArgumentNullException("Jwt Issuer");
        var audience = jwtConfig["Audience"] ?? throw new ArgumentNullException("Jwt Audience");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresIn = int.Parse(jwtConfig["ExpiresInMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(expiresIn),
            signingCredentials: creds
            );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenString;
    }
}
