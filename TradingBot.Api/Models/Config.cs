namespace TradingBot.Api.Models
{
    public class Config
    {
        public decimal MaxDailyBuy { get; set; }
        public decimal MaxDailySell { get; set; }
        public int MaxTradesPerDay { get; set; }
        public bool EnableAutoBuy { get; set; }
        public bool EnableAutoSell { get; set; }
        public bool SignalOnly { get; set; }
        public string[] EnabledTradeTypes { get; set; }
        public decimal RiskRewardRatio { get; set; }
    }
}
