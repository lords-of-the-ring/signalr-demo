using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Server.Auth;

public static class Login
{
    public sealed record Request(string Username, string Password);

    public static IResult Handle(Request request, IConfiguration configuration)
    {
        var signingKeyText = configuration.GetRequiredSection(JwtConstants.SigningKeySection).Value;
        ArgumentNullException.ThrowIfNull(signingKeyText);

        var signingKeyBytes = Encoding.UTF8.GetBytes(signingKeyText);
        var signingKey = new SymmetricSecurityKey(signingKeyBytes);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = JwtConstants.Issuer,
            Audience = JwtConstants.Audience,
            Expires = DateTime.UtcNow.AddSeconds(10),
            SigningCredentials = credentials,
            Subject = new ClaimsIdentity(
            [
                new(JwtRegisteredClaimNames.Sub, request.Username),
            ]),
        };

        var securityToken = new JwtSecurityTokenHandler().CreateToken(tokenDescriptor);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return Results.Ok(new { accessToken });
    }
}
