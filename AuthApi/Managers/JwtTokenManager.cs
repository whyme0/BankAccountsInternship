using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Managers;

public static class JwtTokenManager
{
    public static string CreateJwtToken(CreateJwtTokenDto dto)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, dto.Name)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(dto.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tkn = new JwtSecurityToken(
            issuer: dto.Issuer,
            audience: dto.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tkn);
    }
}