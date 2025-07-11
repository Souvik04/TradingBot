using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TradingBot.Api.Models;
using TradingBot.Api.Services;
using TradingBot.Api.Data;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TradingBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioManagementService _portfolioService;
        private readonly IBrokerService _brokerService;
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(
            IPortfolioManagementService portfolioService,
            IBrokerService brokerService,
            ILogger<PortfolioController> logger)
        {
            _portfolioService = portfolioService;
            _brokerService = brokerService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetPortfolio()
        {
            try
            {
                var holdings = _portfolioService.GetPortfolio();
                var stats = _portfolioService.GetDailyTradeStats();

                return Ok(new
                {
                    Holdings = holdings,
                    CashBalance = _portfolioService.CashBalance,
                    DailyStats = stats,
                    TotalValue = holdings.Sum(h => h.CurrentValue) + _portfolioService.CashBalance
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio");
                return StatusCode(500, "Error retrieving portfolio");
            }
        }

        [HttpGet("holdings")]
        public IActionResult GetHoldings()
        {
            try
            {
                var holdings = _portfolioService.GetPortfolio();
                return Ok(holdings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving holdings");
                return StatusCode(500, "Error retrieving holdings");
            }
        }

        [HttpGet("cash")]
        public IActionResult GetCashBalance()
        {
            try
            {
                return Ok(new { CashBalance = _portfolioService.CashBalance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cash balance");
                return StatusCode(500, "Error retrieving cash balance");
            }
        }

        [HttpGet("stats")]
        public IActionResult GetDailyStats()
        {
            try
            {
                var stats = _portfolioService.GetDailyTradeStats();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily stats");
                return StatusCode(500, "Error retrieving daily stats");
            }
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncWithBroker()
        {
            try
            {
                var brokerHoldings = await _brokerService.GetHoldingsAsync();
                
                // In a real implementation, you'd sync the local portfolio with broker holdings
                // For now, we'll just return the broker holdings
                return Ok(new { 
                    Message = "Portfolio synced with broker",
                    BrokerHoldings = brokerHoldings 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing portfolio with broker");
                return StatusCode(500, "Error syncing portfolio with broker");
            }
        }

        [HttpPost("reset-limits")]
        public IActionResult ResetDailyLimits()
        {
            try
            {
                _portfolioService.ResetDailyLimits();
                return Ok("Daily limits reset successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting daily limits");
                return StatusCode(500, "Error resetting daily limits");
            }
        }

        [HttpGet("analysis")]
        public IActionResult GetPortfolioAnalysis()
        {
            try
            {
                var holdings = _portfolioService.GetPortfolio();
                var totalValue = holdings.Sum(h => h.CurrentValue) + _portfolioService.CashBalance;
                var cashPercentage = totalValue > 0 ? (_portfolioService.CashBalance / totalValue) * 100 : 0;

                var analysis = new
                {
                    TotalValue = totalValue,
                    CashBalance = _portfolioService.CashBalance,
                    CashPercentage = Math.Round(cashPercentage, 2),
                    NumberOfHoldings = holdings.Count,
                    TopHoldings = holdings.OrderByDescending(h => h.CurrentValue).Take(5),
                    DailyStats = _portfolioService.GetDailyTradeStats()
                };

                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating portfolio analysis");
                return StatusCode(500, "Error generating portfolio analysis");
            }
        }
    }
}