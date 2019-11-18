using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using InternationalizationService.Core.Services.Interfaces;

namespace InternationalizationService.Filters
{
    /// <summary>
    /// Middleware to authorize a user
    /// </summary>
    public class AuthorizationMiddleware : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="tokenService"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        /// <param name="systemClock"></param>
        public AuthorizationMiddleware(
            IConfiguration configuration,
            ITokenService tokenService,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock systemClock) : base(options, logger, encoder, systemClock)
        {
            _configuration = configuration;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Determines if a request is authenticated
        /// </summary>
        /// <returns>AuthenticateResult</returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

            ClaimsPrincipal principal;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var jwtToken = authHeader.Parameter;
                if (!authHeader.Scheme.ToLower().Equals("bearer"))
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid Authentication Scheme"));
                }

                principal = _tokenService.GetPrincipal(jwtToken);
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }

            if (principal == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }

            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
