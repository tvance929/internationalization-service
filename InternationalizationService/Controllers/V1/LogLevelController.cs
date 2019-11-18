using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternationalizationService.Config;
using InternationalizationService.Controllers.Base;
using Serilog;

namespace InternationalizationService.Controllers.V1
{
    /// <summary>
    /// The API endpoints to manage the log level of an environment.
    /// </summary>
    [Authorize(Policy = "EmployeeLogin")]
    public class LogLevelController : BaseAPIController
    {
        private static readonly ILogger Logger = Log.ForContext<LogLevelController>();

        /// <summary>
        /// Set the minimum Serilog logging level at runtime.
        /// </summary>
        /// <remarks>
        /// Log level events definition:
        /// 
        ///     1 = Debug
        ///     2 = Information
        ///     3 = Warning
        ///     4 = Error
        /// </remarks>
        /// <param name="logEventLevel">Log event level (corresponds to Serilog.Events.LogEventLevel values)</param>
        /// <returns>The status of the set operation.</returns>
        [HttpPost("SetSerilogLoggingLevel")]
        public ActionResult<string> SetSerilogLoggingLevel(int logEventLevel)
        {
            try
            {
                if (logEventLevel < 1 || logEventLevel > 4)
                {
                    return BadRequest();
                }

                SerilogLoggingLevelSwitch.SetLoggingLevel(logEventLevel);

                return Ok("Success!");
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error setting logging level to {logEventLevel}: {e.Message}");
                return StatusCode(500, $"Error setting logging level to {logEventLevel}: {e.Message}");
            }
        }

        /// <summary>
        /// Get the minimum Serilog logging level at runtime.
        /// </summary>
        /// <remarks>
        /// Log level events definition:
        /// 
        ///     1 = Debug
        ///     2 = Information
        ///     3 = Warning
        ///     4 = Error
        /// </remarks>
        /// <returns>The current minimum level.</returns>
        [HttpGet("GetSerilogLoggingLevel")]
        public ActionResult<int> GetSerilogLoggingLevel()
        {
            try
            {
                return Ok((int)SerilogLoggingLevelSwitch.LevelSwitch.MinimumLevel);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to get logging level: {e.Message}");
                return StatusCode(500, $"Failed to get logging level: {e.Message}");
            }
        }
    }
}