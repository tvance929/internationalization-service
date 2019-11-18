using System;
using System.Linq;
using System.Threading.Tasks;
using InternationalizationService.Core.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using VPT.Caching;
using VPT.Caching.Utility.Interface;
using VPT.Shared.Poco.Model.Accounts.Database;
using VPT.Shared.Rest.Clients.Accounts.Interfaces;

namespace InternationalizationService.Filters
{
    /// <summary>
    /// The authorization handler for EmployeeLogin policy.
    /// </summary>
    public class EmployeeLoginSystemHandler : AuthorizationHandler<EmployeeLoginSystemRequirement>
    {
        private static readonly ILogger Logger = Log.ForContext<EmployeeLoginSystemHandler>();

        private readonly ICacheUtility CacheUtility;
        private readonly IUserAPIClient UserAPIClient;
        private readonly BaseURLOptions BaseURLOptions;
        private readonly ServiceToServiceAuthOptions ServiceToServiceAuthOptions;

        /// <summary>
        /// The constructor of <see cref="EmployeeLoginSystemHandler"/> type handler.
        /// </summary>
        /// <param name="cacheUtility">The cache utility.</param>
        /// <param name="userAPIClient">The user API client.</param>
        /// <param name="baseURLoptions">The base URL options.</param>
        /// <param name="serviceToServiceAuthOptions">The service to service authentication options.</param>
        public EmployeeLoginSystemHandler(ICacheUtility cacheUtility,
            IUserAPIClient userAPIClient,
            IOptions<BaseURLOptions> baseURLoptions,
            IOptions<ServiceToServiceAuthOptions> serviceToServiceAuthOptions)
        {
            CacheUtility = cacheUtility;
            UserAPIClient = userAPIClient;
            BaseURLOptions = baseURLoptions.Value;
            ServiceToServiceAuthOptions = serviceToServiceAuthOptions.Value;
        }

        ///<inheritdoc />
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployeeLoginSystemRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "login_system")
                || !context.User.HasClaim(c => c.Type == "auth_token")
                || !context.User.HasClaim(c => c.Type == "user_external_id"))
            {
                return Task.CompletedTask;
            }

            var loginSystem = Convert.ToInt32(context.User.Claims.FirstOrDefault(m => m.Type == "login_system").Value);
            var authToken = context.User.Claims.FirstOrDefault(m => m.Type == "auth_token").Value;
            var userExternalID = context.User.Claims.FirstOrDefault(m => m.Type == "user_external_id").Value;

            if (loginSystem != (int)requirement.LoginSystem) return Task.CompletedTask;

            var key = CacheUtility.GetKey(RedisModuleKeys.AccountsAPI, RedisObjectKeys.AccountsAuthenticationToken,
                   userExternalID);

            var token = string.Empty;
            try
            {
                var item = CacheUtility.GetCachedItem(key);
                if (!string.IsNullOrEmpty(item))
                {
                    token = JsonConvert.DeserializeObject<Token>(item).SORToken;
                }

                if (!String.IsNullOrWhiteSpace(token) && string.Equals(token, authToken))
                {
                    context.Succeed(requirement);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error retrieving authentication token from the cache: {e.Message}");
            }

            if (!context.HasSucceeded)
            {
                bool isAuthSuccess = UserAPIClient.ValidateVant4geEmployeeAuthenticationToken(userExternalID, authToken, BaseURLOptions.AccountsAPIBaseURL, ServiceToServiceAuthOptions.Username, ServiceToServiceAuthOptions.Password);

                if (isAuthSuccess)
                    context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
