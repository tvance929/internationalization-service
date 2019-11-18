using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace InternationalizationService.Controllers.Base
{
    /// <summary>
    /// The API Controller base implementation.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiController]
    public class BaseAPIController : ControllerBase
    {
        /// <summary>
        /// Generic method to handle exceptions for HttpResponseMessage.
        /// </summary>
        /// <param name="exception">The type of exception.</param>
        /// <param name="userErrorMessage">Safe error message to display to the user</param>
        /// <returns>The error message with HTTP status 500.</returns>
        protected ActionResult<string> HandleException(Exception exception, string userErrorMessage)
        {
#if DEBUG
            return StatusCode((int)HttpStatusCode.InternalServerError, userErrorMessage + ": " + exception.Message + "\n" + exception.StackTrace);
#else
            return StatusCode((int)HttpStatusCode.InternalServerError, userErrorMessage);
#endif
        }
    }
}