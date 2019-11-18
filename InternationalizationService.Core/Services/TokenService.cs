using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using InternationalizationService.Core.Services.Interfaces;
using Serilog;

namespace InternationalizationService.Core.Services
{
    /// <inheritdoc />
    public class TokenService : ITokenService
    {
        private static readonly ILogger Logger = Log.ForContext<TokenService>();
        private const string Secret = "VGhpc0lzQUhpZ2hseVNlY3VyZWRLZXlUb0VuY3J5cHRKV1RQYXlsb2FkLk5vYm9keUNhbkJyZWFrTWUhU29QbGVhc2VEb250VHJ5TWU=";

        /// <inheritdoc />
        public ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                if (jwtToken == null)
                {
                    return null;
                }

                var key = Convert.FromBase64String(Secret);
                var parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, parameters, out var securityToken);

                return principal;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error getting claims principal from token {token}: {e.Message}");
                return null;
            }
        }
    }
}
