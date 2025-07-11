using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TradingBot.Api.Models;
using TradingBot.Api.Services;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace TradingBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradeController : ControllerBase
    {
        private readonly TradeSettings _tradeSettings;
        private readonly IPortfolioManagementService _portfolioService;
        private readonly IBrokerService _brokerService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TradeController> _logger;

        public TradeController(
            IOptions<TradeSettings> tradeSettingsOptions,
            IPortfolioManagementService portfolioService,
            IBrokerService brokerService,
            IHttpClientFactory httpClientFactory,
            ILogger<TradeController> logger)
        {
            _tradeSettings = tradeSettingsOptions.Value;
            _portfolioService = portfolioService;
            _brokerService = brokerService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("config")]
        public IActionResult GetTradeConfig()
        {
            return Ok(_tradeSettings);
        }

        [HttpPost("config")]
        public IActionResult UpdateTradeConfig([FromBody] TradeSettings newSettings)
        {
            // In a real implementation, you'd want to persist this to database
            // For now, we'll just return the updated settings
            return Ok(newSettings);
        }

        [HttpGet("portfolio")]
        public IActionResult GetPortfolio()
        {
            var portfolio = _portfolioService.GetPortfolio();
            var stats = _portfolioService.GetDailyTradeStats();
            
            return Ok(new
            {
                Holdings = portfolio,
                CashBalance = _portfolioService.CashBalance,
                DailyStats = stats
            });
        }

        [HttpPost("signal")]
        public async Task<IActionResult> GenerateSignal([FromBody] SignalEngineInput input)
        {
            try
            {
                if (input.Symbols == null || input.Symbols.Length == 0)
                {
                    return BadRequest("At least one symbol is required");
                }

                var signals = new List<object>();
                var httpClient = _httpClientFactory.CreateClient();

                foreach (var symbol in input.Symbols)
                {
                    var signalRequest = new { symbol = symbol };
                    var json = JsonSerializer.Serialize(signalRequest);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Call the Python signal engine
                    var response = await httpClient.PostAsync("http://signal-engine:8000/signal", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var signal = JsonSerializer.Deserialize<object>(responseContent);
                        signals.Add(signal);
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to get signal for {symbol}: {response.StatusCode}");
                    }
                }

                return Ok(signals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating signals");
                return StatusCode(500, "Error generating signals");
            }
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteTrade([FromBody] TradeExecutionRequest request)
        {
            try
            {
                // Validate trade settings
                if (_tradeSettings.EnableSignalOnly)
                {
                    return BadRequest("Trading is disabled. Signal-only mode is enabled.");
                }

                if (!_tradeSettings.TradeTypesEnabled.Contains(request.TradeType))
                {
                    return BadRequest($"Trade type '{request.TradeType}' is not enabled.");
                }

                // Check portfolio constraints
                var tradeType = Enum.Parse<TradeType>(request.TradeType, true);
                var decision = request.Side.ToLower() == "buy" 
                    ? _portfolioService.CanBuy(request.Symbol, request.Quantity, request.Price, tradeType)
                    : _portfolioService.CanSell(request.Symbol, request.Quantity, tradeType);

                if (!decision.CanExecute)
                {
                    return BadRequest($"Trade cannot be executed: {decision.Reason}");
                }

                // Place order with broker
                var orderId = await _brokerService.PlaceOrderAsync(
                    request.Symbol, 
                    request.Quantity, 
                    request.Price, 
                    request.Side, 
                    request.OrderType);

                // Update portfolio
                _portfolioService.ApplyTrade(
                    request.Symbol, 
                    request.Quantity, 
                    request.Price, 
                    tradeType, 
                    request.Side.ToLower() == "buy");

                return Ok(new { OrderId = orderId, Message = "Trade executed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing trade");
                return StatusCode(500, "Error executing trade");
            }
        }

        [HttpGet("stats")]
        public IActionResult GetTradeStats()
        {
            var stats = _portfolioService.GetDailyTradeStats();
            return Ok(stats);
        }

        [HttpPost("reset-limits")]
        public IActionResult ResetDailyLimits()
        {
            _portfolioService.ResetDailyLimits();
            return Ok("Daily limits reset successfully");
        }
    }

    public class TradeExecutionRequest
    {
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Side { get; set; } // "buy" or "sell"
        public string OrderType { get; set; } // "market", "limit", "stop"
        public string TradeType { get; set; } // "intraday", "swing", "longterm"
    }
}