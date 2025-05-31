using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TradingBot.Api.Models;

namespace TradingBot.Api.Controllers
{
    [ApiController]
    [Route("signals")]
    public class TradeController : ControllerBase
    {
        private readonly TradeSettings _tradeSettings;

        public TradeController(IOptions<TradeSettings> tradeSettingsOptions)
        {
            _tradeSettings = tradeSettingsOptions.Value;
        }

        [HttpGet("config")]
        public IActionResult GetTradeConfig()
        {
            return Ok(_tradeSettings);
        }

        // Example usage in your trade logic
        [HttpPost("generate")]
        public IActionResult GenerateSignal([FromBody] SignalEngineInput input)
        {
            if (_tradeSettings.EnableSignalOnly)
            {
                // Only generate signal, don't execute trades
            }

            if (_tradeSettings.EnableAutoBuy && input.TradeType == "buy")
            {
                // Allow auto-buy logic
            }

            if (_tradeSettings.EnableAutoSell && input.TradeType == "sell")
            {
                // Allow auto-sell logic
            }

            if (!_tradeSettings.TradeTypesEnabled.Contains(input.TradeType))
            {
                return BadRequest("Trade type not enabled.");
            }

            // Use _tradeSettings.MaxDailyBuyAmount, etc. as needed

            // ...the rest of your logic...
            return Ok();
        }
    }
}