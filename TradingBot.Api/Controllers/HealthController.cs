using Microsoft.AspNetCore.Mvc;

namespace TradingBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Status = "Healthy",
                Service = "TradingBot API",
                Version = "1.0.0",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("ready")]
        public IActionResult Ready()
        {
            // Add any readiness checks here (database, external services, etc.)
            return Ok(new
            {
                Status = "Ready",
                Service = "TradingBot API",
                Timestamp = DateTime.UtcNow
            });
        }
    }
} 