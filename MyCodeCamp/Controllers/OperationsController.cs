using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class OperationsController : Controller
    {
        private readonly IConfigurationRoot _config;
        private readonly ILogger<OperationsController> _logger;

        public OperationsController(IConfigurationRoot config, ILogger<OperationsController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpOptions("reloadConfig")]
        public IActionResult ReloadConfiguratio()
        {
            try
            {
                _config.Reload();

                return Ok("Configuration reloaded");
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception thron while reloading configuration: {exception}");
            }

            return BadRequest("Could not reload configuration");
        }
    }
}
