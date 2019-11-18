using System.Security.Claims;

namespace InternationalizationService.Core.Services.Interfaces
{
    /// <summary>
    /// JWT service for managing tokens
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generate the user's identity from a JWT
        /// </summary>
        /// <param name="token">JWT used to generate identity</param>
        /// <returns>ClaimsPrincipal with user identity</returns>
        ClaimsPrincipal GetPrincipal(string token);
    }
}
