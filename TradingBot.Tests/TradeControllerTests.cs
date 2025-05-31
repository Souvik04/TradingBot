using Xunit;
using Microsoft.AspNetCore.Mvc;
using TradingBot.Api.Models;

namespace TradingBot.Tests
{
    public class TradeControllerTests
    {
        [Fact]
        public void TestGetConfig()
        {
            var controller = new TradeController();
            var result = controller.GetConfig() as OkObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void TestPostSignal()
        {
            var controller = new TradeController();
            var signal = new TradeSignal
            {
                Symbol = "RELIANCE.NS",
                Action = "Buy",
                TradeType = "Intraday",
                Target = 2500,
                StopLoss = 2400
            };
            var result = controller.PostSignal(signal) as OkObjectResult;
            Assert.NotNull(result);
            Assert.Contains("Signal Received", result?.Value?.ToString());
        }
    }
}
