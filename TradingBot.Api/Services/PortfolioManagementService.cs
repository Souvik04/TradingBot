using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingBot.Api.Data;
using TradingBot.Api.Models;

namespace TradingBot.Api.Services
{
    public class PortfolioManagementService : IPortfolioManagementService
    {
        private readonly ILogger<PortfolioManagementService> _logger;
        private readonly TradeSettings _settings;
        private readonly TradingBotDbContext _dbContext;
        private readonly object _lock = new();

        private readonly List<Holding> _holdings;
        private decimal _cashBalance;

        public PortfolioManagementService(
            IOptions<TradeSettings> settings,
            TradingBotDbContext dbContext,
            ILogger<PortfolioManagementService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            _dbContext = dbContext;
            _holdings = new List<Holding>
            {
                new Holding {Symbol = "INFY", Quantity = 10, AveragePrice = 1500, LastTradedPrice = 1540, LastUpdated = DateTime.Now},
                new Holding {Symbol = "TCS", Quantity = 5, AveragePrice = 3200, LastTradedPrice = 3225, LastUpdated = DateTime.Now}
            };
            _cashBalance = 100000;
            EnsureTodayStats().Wait();
        }

        private async Task EnsureTodayStats()
        {
            var today = DateTime.UtcNow.Date;
            var stats = await _dbContext.DailyTradeStats.FindAsync(today);
            if (stats == null)
            {
                stats = new DailyTradeStats
                {
                    Date = today,
                    BuyAmount = 0,
                    SellAmount = 0,
                    TradeCount = 0
                };
                _dbContext.DailyTradeStats.Add(stats);
                await _dbContext.SaveChangesAsync();
            }
        }

        private DailyTradeStats GetTodayStats()
        {
            var today = DateTime.UtcNow.Date;
            return _dbContext.DailyTradeStats.First(s => s.Date == today);
        }

        public IReadOnlyList<Holding> GetPortfolio() => _holdings.ToList();
        public decimal CashBalance => _cashBalance;

        public DailyTradeStats GetDailyTradeStats()
        {
            lock (_lock)
            {
                return GetTodayStats();
            }
        }

        public TradeDecisionResult CanBuy(string symbol, int quantity, decimal price, TradeType tradeType, string user = null)
        {
            lock (_lock)
            {
                var stats = GetTodayStats();

                if (_settings.EnableSignalOnly)
                    return DenyAndAudit("Signal-only mode enabled.", symbol, quantity, price, tradeType, true, user);

                if (!_settings.EnableAutoBuy)
                    return DenyAndAudit("Auto-buy disabled in config.", symbol, quantity, price, tradeType, true, user);

                if (!IsTradeTypeEnabled(tradeType))
                    return DenyAndAudit("Trade type not enabled in config.", symbol, quantity, price, tradeType, true, user);

                var cost = quantity * price;
                if (cost > _cashBalance)
                    return DenyAndAudit("Insufficient cash for buy.", symbol, quantity, price, tradeType, true, user);

                if (stats.BuyAmount + cost > _settings.MaxDailyBuyAmount)
                    return DenyAndAudit("Max daily buy amount exceeded.", symbol, quantity, price, tradeType, true, user);

                if (stats.TradeCount + 1 > _settings.MaxDailyTrades)
                    return DenyAndAudit("Max daily trades exceeded.", symbol, quantity, price, tradeType, true, user);

                Audit("Allowed", null, symbol, quantity, price, tradeType, true, user);
                return TradeDecisionResult.Allowed();
            }
        }

        public TradeDecisionResult CanSell(string symbol, int quantity, TradeType tradeType, string user = null)
        {
            lock (_lock)
            {
                var stats = GetTodayStats();

                if (_settings.EnableSignalOnly)
                    return DenyAndAudit("Signal-only mode enabled.", symbol, quantity, 0, tradeType, false, user);

                if (!_settings.EnableAutoSell)
                    return DenyAndAudit("Auto-sell disabled in config.", symbol, quantity, 0, tradeType, false, user);

                if (!IsTradeTypeEnabled(tradeType))
                    return DenyAndAudit("Trade type not enabled in config.", symbol, quantity, 0, tradeType, false, user);

                var holding = _holdings.FirstOrDefault(h => h.Symbol == symbol);
                if (holding == null || holding.Quantity < quantity)
                    return DenyAndAudit("Insufficient holdings for sell.", symbol, quantity, 0, tradeType, false, user);

                var sellValue = quantity * holding.LastTradedPrice;
                if (stats.SellAmount + sellValue > _settings.MaxDailySellAmount)
                    return DenyAndAudit("Max daily sell amount exceeded.", symbol, quantity, holding.LastTradedPrice, tradeType, false, user);

                if (stats.TradeCount + 1 > _settings.MaxDailyTrades)
                    return DenyAndAudit("Max daily trades exceeded.", symbol, quantity, holding.LastTradedPrice, tradeType, false, user);

                Audit("Allowed", null, symbol, quantity, holding.LastTradedPrice, tradeType, false, user);
                return TradeDecisionResult.Allowed();
            }
        }

        public void ApplyTrade(string symbol, int quantity, decimal price, TradeType tradeType, bool isBuy, string user = null)
        {
            lock (_lock)
            {
                var stats = GetTodayStats();
                stats.TradeCount++;
                if (isBuy)
                {
                    stats.BuyAmount += quantity * price;
                    _cashBalance -= quantity * price;
                    var holding = _holdings.FirstOrDefault(h => h.Symbol == symbol);
                    if (holding == null)
                    {
                        _holdings.Add(new Holding
                        {
                            Symbol = symbol,
                            Quantity = quantity,
                            AveragePrice = price,
                            LastTradedPrice = price,
                            LastUpdated = DateTime.Now
                        });
                    }
                    else
                    {
                        var totalCost = (holding.Quantity * holding.AveragePrice) + (quantity * price);
                        holding.Quantity += quantity;
                        holding.AveragePrice = totalCost / holding.Quantity;
                        holding.LastTradedPrice = price;
                        holding.LastUpdated = DateTime.Now;
                    }
                    _logger.LogInformation("Applied BUY: {Quantity} {Symbol} at {Price}", quantity, symbol, price);
                }
                else
                {
                    stats.SellAmount += quantity * price;
                    _cashBalance += quantity * price;
                    var holding = _holdings.FirstOrDefault(h => h.Symbol == symbol);
                    if (holding != null)
                    {
                        holding.Quantity -= quantity;
                        holding.LastTradedPrice = price;
                        holding.LastUpdated = DateTime.Now;
                        if (holding.Quantity <= 0)
                            _holdings.Remove(holding);
                    }
                    _logger.LogInformation("Applied SELL: {Quantity} {Symbol} at {Price}", quantity, symbol, price);
                }
                _dbContext.SaveChanges();
            }
        }

        public void ResetDailyLimits(string user = null)
        {
            lock (_lock)
            {
                var today = DateTime.UtcNow.Date;
                var stats = _dbContext.DailyTradeStats.FirstOrDefault(s => s.Date == today);
                if (stats != null)
                {
                    stats.BuyAmount = 0;
                    stats.SellAmount = 0;
                    stats.TradeCount = 0;
                    _dbContext.SaveChanges();
                    Audit("Reset", "Manual or scheduled reset", null, 0, 0, TradeType.None, false, user);
                    _logger.LogInformation("Daily limits and counters reset.");
                }
            }
        }

        private TradeDecisionResult DenyAndAudit(string reason, string symbol, int quantity, decimal price, TradeType tradeType, bool isBuy, string user)
        {
            Audit("Blocked", reason, symbol, quantity, price, tradeType, isBuy, user);
            return TradeDecisionResult.Denied(reason);
        }

        private void Audit(string decision, string reason, string symbol, int quantity, decimal price, TradeType tradeType, bool isBuy, string user)
        {
            _dbContext.AuditLogs.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                Symbol = symbol,
                Quantity = quantity,
                Price = price,
                TradeType = tradeType.ToString(),
                IsBuy = isBuy,
                Decision = decision,
                Reason = reason ?? "",
                User = user ?? ""
            });
            _dbContext.SaveChanges();
        }

        private bool IsTradeTypeEnabled(TradeType tradeType)
        {
            if (_settings.TradeTypesEnabled == null || !_settings.TradeTypesEnabled.Any())
                return false;

            foreach (var enabledTypeStr in _settings.TradeTypesEnabled)
            {
                if (Enum.TryParse<TradeType>(enabledTypeStr, true, out var enabledType)
                    && tradeType.HasFlag(enabledType))
                    return true;
            }
            return false;
        }
    }
}