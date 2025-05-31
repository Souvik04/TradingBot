using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TradingBot.Api.Models;
using TradingBot.Api.Services;
using TradingBot.Api.Data;
using Microsoft.Extensions.Options;

namespace TradingBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioManagementService _portfolioService;
        private readonly TradeSettings _settings;
        private readonly TradingBotDbContext _dbContext;

        public PortfolioController(IPortfolioManagementService portfolioService, IOptions<TradeSettings> settings, TradingBotDbContext dbContext)
        {
            _portfolioService = portfolioService;
            _settings = settings.Value;
            _dbContext = dbContext;
        }

        [HttpGet("stats")]
        public ActionResult<DailyTradeStats> GetStats()
        {
            return Ok(_portfolioService.GetDailyTradeStats());
        }

        [HttpGet("limits")]
        public ActionResult<TradeSettings> GetLimits()
        {
            return Ok(_settings);
        }

        [HttpPost("reset")]
        public IActionResult ResetStats()
        {
            _portfolioService.ResetDailyLimits("api");
            return Ok();
        }

        [HttpGet("audit")]
        public IActionResult GetAuditLog([FromQuery] DateTime? date = null)
        {
            var logs = _dbContext.AuditLogs
                .Where(a => !date.HasValue || a.Timestamp.Date == date.Value.Date)
                .OrderByDescending(a => a.Timestamp)
                .ToList();
            return Ok(logs);
        }
    }
}