using System.Collections.Generic;

namespace TradingBot.Api.Models
{
    public class TradeSettings
    {
        public bool EnableAutoBuy { get; set; }
        public bool EnableAutoSell { get; set; }
        public bool EnableSignalOnly { get; set; }
        public decimal MaxDailyBuyAmount { get; set; }
        public decimal MaxDailySellAmount { get; set; }
        public int MaxDailyTrades { get; set; }
        public List<string> TradeTypesEnabled { get; set; }
        public string DefaultMode { get; set; }
    }
}