using System;

namespace TradingBot.Api.Models
{
    public class Holding
    {
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal LastTradedPrice { get; set; } // Mocked, updatable
        public DateTime LastUpdated { get; set; }

        public decimal UnrealizedPnL => (LastTradedPrice - AveragePrice) * Quantity;
    }
}