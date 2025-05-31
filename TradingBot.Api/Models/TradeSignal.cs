namespace TradingBot.Api.Models
{
    public class TradeSignal
    {
        public string Symbol { get; set; }
        public string Action { get; set; } // Buy or Sell
        public string TradeType { get; set; } // Intraday, Swing, etc.
        public decimal Target { get; set; }
        public decimal StopLoss { get; set; }
    }
}
