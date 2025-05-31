using System.Collections.Generic;
using TradingBot.Api.Models;

namespace TradingBot.Api.Services
{
    public interface IPortfolioManagementService
    {
        IReadOnlyList<Holding> GetPortfolio();
        decimal CashBalance { get; }
        TradeDecisionResult CanBuy(string symbol, int quantity, decimal price, TradeType tradeType);
        TradeDecisionResult CanSell(string symbol, int quantity, TradeType tradeType);
        void ApplyTrade(string symbol, int quantity, decimal price, TradeType tradeType, bool isBuy);
        decimal DailyBuyAmount { get; }
        decimal DailySellAmount { get; }
        int DailyTradeCount { get; }
        DailyTradeStats GetDailyTradeStats();
        void ResetDailyLimits(string user = null);
    }
}