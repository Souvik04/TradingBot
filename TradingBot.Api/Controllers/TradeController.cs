using Microsoft.AspNetCore.Mvc;
using TradingBot.Api.Models;
using TradingBot.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class TradeController : ControllerBase
{
    [HttpGet("config")]
    public IActionResult GetConfig() => Ok(ConfigManager.Get());

    [HttpPost("config")]
    public IActionResult UpdateConfig([FromBody] Config config)
    {
        ConfigManager.Save(config);
        return Ok();
    }

    [HttpPost("signal")] // For manual testing
    public IActionResult PostSignal([FromBody] TradeSignal signal)
    {
        return Ok(TradeExecutor.HandleSignal(signal));
    }
}