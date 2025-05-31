using TradingBot.Api.Models;

namespace TradingBot.Api.Services
{
    public static class TradeExecutor
    {
        public static string HandleSignal(TradeSignal signal)
        {
            // Mock logic
            return $"Signal Received: {signal.Symbol} -> {signal.Action}";
        }
    }
}
