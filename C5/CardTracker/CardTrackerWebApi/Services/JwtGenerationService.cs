using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using CardTrackerWebApi.Settings;

namespace CardTrackerWebApi.Services;

public class JwtGenerationService(IOptionsSnapshot<AuthSettings> jwtSettings)
: ITokenGenerationService
{
    private readonly AuthSettings _jwtSettings = jwtSettings.Value;

    public string GenerateToken(string username, string role)
    {
        string secret = _jwtSettings.Secret;
        byte[] keyBytes = Encoding.UTF8.GetBytes((secret));
        SymmetricSecurityKey key = new(keyBytes);
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityTokenHandler()
            .WriteToken(new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: [
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.Name, username)
                ],
                expires: DateTime.Now.AddDays(7),
            signingCredentials: credentials));
    }
}