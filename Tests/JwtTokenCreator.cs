using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Tests;

internal static class JwtTokenCreator
{
    public static string CreateTestJwtToken()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test")
        };

        var key = new SymmetricSecurityKey("this-is-should-be-pretty-secure-secret-key"u8.ToArray());
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "BankAccountsAuthApi",
            audience: "BankAccountsApi",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}