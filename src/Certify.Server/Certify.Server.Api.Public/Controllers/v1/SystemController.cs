﻿using Certify.Client;
using Certify.Models.Hub;
using Microsoft.AspNetCore.Mvc;

namespace Certify.Server.Api.Public.Controllers
{
    /// <summary>
    /// Provides general system level information (version etc)
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public partial class SystemController : ApiControllerBase
    {

        private readonly ILogger<SystemController> _logger;

        private readonly ICertifyInternalApiClient _client;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="client"></param>
        public SystemController(ILogger<SystemController> logger, ICertifyInternalApiClient client)
        {
            _logger = logger;
            _client = client;
        }

        /// <summary>
        /// Get the server software version
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("version")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VersionInfo))]
        public async Task<IActionResult> GetSystemVersion()
        {
            var versionInfo = await _client.GetAppVersion();
            var result = new Models.Hub.VersionInfo { Version = versionInfo, Product = "Certify Management Hub" };
            return new OkObjectResult(result);
        }

        /// <summary>
        /// Check API is responding and can connect to background service
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("health")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
        public async Task<IActionResult> GetHealth()
        {
            var serviceAvailable = false;
            var versionInfo = "Not available. Cannot connect to core service.";
            try
            {
                versionInfo = await _client.GetAppVersion();
                serviceAvailable = true;
            }
            catch { }

#if DEBUG
            var health = new { API = "OK", Service = versionInfo, ServiceAvailable = serviceAvailable, env = Environment.GetEnvironmentVariables() };
#else
            var health = new { API = "OK", Service = versionInfo, ServiceAvailable = serviceAvailable};
#endif

            return new OkObjectResult(health);
        }
    }
}
