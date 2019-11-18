using Microsoft.AspNetCore.Authorization;
using VPT.Shared.Poco.Enum.Accounts;

namespace InternationalizationService.Filters
{
    /// <summary>
    /// The authorization requirements for EmployeeLogin policy.
    /// </summary>
    public class EmployeeLoginSystemRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// The supported LoginSystem.
        /// </summary>
        public LoginSystem LoginSystem { get; }

        /// <summary>
        /// The constructory of <see cref="EmployeeLoginSystemRequirement"/> type.
        /// </summary>
        /// <param name="loginSystem"></param>
        public EmployeeLoginSystemRequirement(LoginSystem loginSystem)
        {
            this.LoginSystem = loginSystem;
        }
    }
}
