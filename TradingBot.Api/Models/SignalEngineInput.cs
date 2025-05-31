namespace TradingBot.Api.Models
{
    public class SignalEngineInput
    {
        public string[] Symbols { get; set; }
        public string TradeType { get; set; }
        public string StrategyName { get; set; }
        public string TradeMode { get; set; } // "paper", "live", "signal", "backtest"
    }
}