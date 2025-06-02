using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Web.Signalr;

namespace Web.Auth;

public static class AuthRegistration
{
    public static IServiceCollection AddAuth(this IServiceCollection services,
        IConfiguration configuration)
    {
        var signingKey = configuration.GetRequiredSection(JwtConstants.SigningKeySection).Value;

        ArgumentNullException.ThrowIfNull(signingKey);

        var signingKeyBytes = Encoding.UTF8.GetBytes(signingKey);

        services
            .AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtConstants.Issuer,
                    ValidAudience = JwtConstants.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context
                            .Request
                            .Query[SignalrConstants.AccessTokenQueryParameter];

                        var path = context.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(SignalrConstants.HubsBasePrefix))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
