namespace Forex.Infrastructure.Security;

using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtTokenGenerator(IConfiguration config) : IJwtTokenGenerator
{
    public string GenerateToken(User user, IList<string> roles)
    {
        var claims = new List<Claim>();

        if (!string.IsNullOrWhiteSpace(user.Name))
            claims.Add(new Claim(ClaimTypes.Name, user.Name));

        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

        if (!string.IsNullOrWhiteSpace(user.Email))
            claims.Add(new Claim(ClaimTypes.Email, user.Email));

        if (!string.IsNullOrWhiteSpace(user.Phone))
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.Phone));

        if (roles is not null && roles.Count > 0)
        {
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!)
        );
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
