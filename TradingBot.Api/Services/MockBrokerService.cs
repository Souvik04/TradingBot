using System;
using System.Threading.Tasks;
using TradingBot.Api.Models;
using Microsoft.Extensions.Logging;

namespace TradingBot.Api.Services
{
    public class MockBrokerService : IBrokerService
    {
        private readonly ILogger<MockBrokerService> _logger;

        public MockBrokerService(ILogger<MockBrokerService> logger)
        {
            _logger = logger;
        }

        public Task<string> PlaceOrderAsync(string symbol, int quantity, decimal price, string side, string orderType)
        {
            _logger.LogInformation("Mock PlaceOrder: {Side} {Quantity} {Symbol} at {Price} as {OrderType}",
                side, quantity, symbol, price, orderType);
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public Task<string> GetOrderStatusAsync(string orderId)
        {
            _logger.LogInformation("Mock GetOrderStatus: {OrderId}", orderId);
            return Task.FromResult("Filled");
        }

        public Task<Holding[]> GetHoldingsAsync()
        {
            _logger.LogInformation("Mock GetHoldings called.");
            return Task.FromResult(Array.Empty<Holding>());
        }
    }
}