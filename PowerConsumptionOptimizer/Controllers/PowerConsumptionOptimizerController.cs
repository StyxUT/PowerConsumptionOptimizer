using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using PowerConsumptionOptimizer.Controllers;
using PowerConsumptionOptimizer.Models;
using PowerConsumptionOptimizer.Services;

namespace PowerConsumptionOptimizer.Controllers
{
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ApiController]
    public class PowerConsumptionOptimizerController : ControllerBase
    {
        private readonly ILogger<PowerConsumptionOptimizerController> _logger;
        private readonly IPowerConsumptionOptimizerService _pcoService;

        // RetryPolicy _retryPolicy;

        public PowerConsumptionOptimizerController(ILogger<PowerConsumptionOptimizerController> logger, IPowerConsumptionOptimizerService pcoService)
        {
            _pcoService = pcoService;
            _logger = logger;
            _logger.LogDebug("PowerConsumptionOptimizer Controller instanciated");
        }

        /// <summary>
        /// HealthCheck
        /// </summary>
        /// <returns>Returns 200 (Ok)</returns>
        [HttpGet("HealthCheck")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public OkResult HealthCheck()
        {
            _logger.LogDebug("HealthCheck called");
            return Ok();
        }
    }
}