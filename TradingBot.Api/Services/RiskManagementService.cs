using System;
using System.Collections.Generic;
using System.Linq;
using TradingBot.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TradingBot.Api.Services
{
    public interface IRiskManagementService
    {
        RiskAssessment AssessTradeRisk(string symbol, int quantity, decimal price, string side, TradeType tradeType);
        bool ValidatePositionSize(string symbol, int quantity, decimal price);
        bool ValidateSectorExposure(string symbol, int quantity, decimal price);
        bool ValidateDrawdown(decimal potentialLoss);
        decimal CalculatePositionSize(string symbol, decimal availableCapital);
        RiskMetrics GetCurrentRiskMetrics();
    }

    public class RiskManagementService : IRiskManagementService
    {
        private readonly ILogger<RiskManagementService> _logger;
        private readonly IPortfolioManagementService _portfolioService;
        private readonly RiskConfig _config;

        public RiskManagementService(
            IOptions<RiskConfig> config,
            IPortfolioManagementService portfolioService,
            ILogger<RiskManagementService> logger)
        {
            _config = config.Value;
            _portfolioService = portfolioService;
            _logger = logger;
        }

        public RiskAssessment AssessTradeRisk(string symbol, int quantity, decimal price, string side, TradeType tradeType)
        {
            var assessment = new RiskAssessment
            {
                Symbol = symbol,
                Quantity = quantity,
                Price = price,
                Side = side,
                TradeType = tradeType,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                var tradeValue = quantity * price;
                var portfolio = _portfolioService.GetPortfolio();
                var totalPortfolioValue = portfolio.Sum(h => h.CurrentValue) + _portfolioService.CashBalance;

                // 1. Position Size Risk
                assessment.PositionSizeRisk = AssessPositionSizeRisk(tradeValue, totalPortfolioValue);

                // 2. Sector Concentration Risk
                assessment.SectorConcentrationRisk = AssessSectorConcentrationRisk(symbol, tradeValue, portfolio);

                // 3. Daily Loss Risk
                assessment.DailyLossRisk = AssessDailyLossRisk(tradeValue, side);

                // 4. Volatility Risk (if we have historical data)
                assessment.VolatilityRisk = AssessVolatilityRisk(symbol, price);

                // 5. Liquidity Risk
                assessment.LiquidityRisk = AssessLiquidityRisk(symbol, quantity);

                // Calculate overall risk score
                assessment.OverallRiskScore = CalculateOverallRiskScore(assessment);

                // Determine if trade should be allowed
                assessment.IsTradeAllowed = assessment.OverallRiskScore <= _config.MaxRiskScore;

                _logger.LogInformation($"Risk assessment for {symbol}: Score={assessment.OverallRiskScore}, Allowed={assessment.IsTradeAllowed}");

                return assessment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assessing risk for {symbol}");
                assessment.IsTradeAllowed = false;
                assessment.RiskReason = "Error in risk assessment";
                return assessment;
            }
        }

        public bool ValidatePositionSize(string symbol, int quantity, decimal price)
        {
            var tradeValue = quantity * price;
            var portfolio = _portfolioService.GetPortfolio();
            var totalPortfolioValue = portfolio.Sum(h => h.CurrentValue) + _portfolioService.CashBalance;

            // Check maximum position size as percentage of portfolio
            var positionPercentage = tradeValue / totalPortfolioValue;
            if (positionPercentage > _config.MaxPositionSizePercentage)
            {
                _logger.LogWarning($"Position size {positionPercentage:P2} exceeds maximum {_config.MaxPositionSizePercentage:P2}");
                return false;
            }

            // Check absolute position size limit
            if (tradeValue > _config.MaxPositionSizeAbsolute)
            {
                _logger.LogWarning($"Position value {tradeValue:C} exceeds maximum {_config.MaxPositionSizeAbsolute:C}");
                return false;
            }

            return true;
        }

        public bool ValidateSectorExposure(string symbol, int quantity, decimal price)
        {
            var tradeValue = quantity * price;
            var portfolio = _portfolioService.GetPortfolio();
            var totalPortfolioValue = portfolio.Sum(h => h.CurrentValue) + _portfolioService.CashBalance;

            // Get sector for the symbol (this would need sector mapping)
            var sector = GetSectorForSymbol(symbol);
            var currentSectorExposure = CalculateSectorExposure(sector, portfolio);
            var newSectorExposure = currentSectorExposure + tradeValue;

            var sectorPercentage = newSectorExposure / totalPortfolioValue;
            if (sectorPercentage > _config.MaxSectorExposurePercentage)
            {
                _logger.LogWarning($"Sector exposure {sectorPercentage:P2} exceeds maximum {_config.MaxSectorExposurePercentage:P2}");
                return false;
            }

            return true;
        }

        public bool ValidateDrawdown(decimal potentialLoss)
        {
            var portfolio = _portfolioService.GetPortfolio();
            var totalPortfolioValue = portfolio.Sum(h => h.CurrentValue) + _portfolioService.CashBalance;
            var currentDrawdown = CalculateCurrentDrawdown(portfolio);
            var newDrawdown = currentDrawdown + potentialLoss;

            if (newDrawdown > _config.MaxDrawdownAmount)
            {
                _logger.LogWarning($"Drawdown {newDrawdown:C} would exceed maximum {_config.MaxDrawdownAmount:C}");
                return false;
            }

            return true;
        }

        public decimal CalculatePositionSize(string symbol, decimal availableCapital)
        {
            // Kelly Criterion or fixed percentage approach
            var positionSize = availableCapital * _config.PositionSizePercentage;
            
            // Apply maximum position size limit
            positionSize = Math.Min(positionSize, _config.MaxPositionSizeAbsolute);
            
            return positionSize;
        }

        public RiskMetrics GetCurrentRiskMetrics()
        {
            var portfolio = _portfolioService.GetPortfolio();
            var totalPortfolioValue = portfolio.Sum(h => h.CurrentValue) + _portfolioService.CashBalance;

            return new RiskMetrics
            {
                TotalPortfolioValue = totalPortfolioValue,
                CashPercentage = _portfolioService.CashBalance / totalPortfolioValue,
                NumberOfPositions = portfolio.Count,
                LargestPositionPercentage = portfolio.Any() ? portfolio.Max(h => h.CurrentValue) / totalPortfolioValue : 0,
                CurrentDrawdown = CalculateCurrentDrawdown(portfolio),
                DailyPnL = portfolio.Sum(h => h.PnL),
                SectorExposures = CalculateSectorExposures(portfolio),
                Timestamp = DateTime.UtcNow
            };
        }

        private RiskLevel AssessPositionSizeRisk(decimal tradeValue, decimal totalPortfolioValue)
        {
            var percentage = tradeValue / totalPortfolioValue;
            
            if (percentage <= _config.LowRiskPositionPercentage)
                return RiskLevel.Low;
            else if (percentage <= _config.MediumRiskPositionPercentage)
                return RiskLevel.Medium;
            else
                return RiskLevel.High;
        }

        private RiskLevel AssessSectorConcentrationRisk(string symbol, decimal tradeValue, List<Holding> portfolio)
        {
            var sector = GetSectorForSymbol(symbol);
            var currentExposure = CalculateSectorExposure(sector, portfolio);
            var totalValue = portfolio.Sum(h => h.CurrentValue) + _portfolioService.CashBalance;
            var newExposure = (currentExposure + tradeValue) / totalValue;

            if (newExposure <= _config.LowRiskSectorPercentage)
                return RiskLevel.Low;
            else if (newExposure <= _config.MediumRiskSectorPercentage)
                return RiskLevel.Medium;
            else
                return RiskLevel.High;
        }

        private RiskLevel AssessDailyLossRisk(decimal tradeValue, string side)
        {
            var dailyStats = _portfolioService.GetDailyTradeStats();
            var potentialLoss = side.ToLower() == "buy" ? tradeValue * _config.MaxDailyLossPercentage : 0;
            var newDailyLoss = dailyStats.DailyPnL - potentialLoss;

            if (Math.Abs(newDailyLoss) <= _config.LowRiskDailyLossAmount)
                return RiskLevel.Low;
            else if (Math.Abs(newDailyLoss) <= _config.MediumRiskDailyLossAmount)
                return RiskLevel.Medium;
            else
                return RiskLevel.High;
        }

        private RiskLevel AssessVolatilityRisk(string symbol, decimal price)
        {
            // This would require historical price data
            // For now, return medium risk
            return RiskLevel.Medium;
        }

        private RiskLevel AssessLiquidityRisk(string symbol, int quantity)
        {
            // This would require volume data
            // For now, return low risk for small quantities
            return quantity <= 1000 ? RiskLevel.Low : RiskLevel.Medium;
        }

        private double CalculateOverallRiskScore(RiskAssessment assessment)
        {
            var score = 0.0;
            
            score += GetRiskScore(assessment.PositionSizeRisk) * _config.PositionSizeWeight;
            score += GetRiskScore(assessment.SectorConcentrationRisk) * _config.SectorConcentrationWeight;
            score += GetRiskScore(assessment.DailyLossRisk) * _config.DailyLossWeight;
            score += GetRiskScore(assessment.VolatilityRisk) * _config.VolatilityWeight;
            score += GetRiskScore(assessment.LiquidityRisk) * _config.LiquidityWeight;

            return score;
        }

        private double GetRiskScore(RiskLevel riskLevel)
        {
            return riskLevel switch
            {
                RiskLevel.Low => 0.2,
                RiskLevel.Medium => 0.5,
                RiskLevel.High => 0.8,
                _ => 0.5
            };
        }

        private string GetSectorForSymbol(string symbol)
        {
            // This would need a sector mapping database or API
            // For now, return a default sector
            return "Technology";
        }

        private decimal CalculateSectorExposure(string sector, List<Holding> portfolio)
        {
            // This would need sector mapping for each holding
            // For now, return 0
            return 0;
        }

        private decimal CalculateCurrentDrawdown(List<Holding> portfolio)
        {
            return portfolio.Sum(h => h.PnL < 0 ? Math.Abs(h.PnL) : 0);
        }

        private Dictionary<string, decimal> CalculateSectorExposures(List<Holding> portfolio)
        {
            // This would need sector mapping for each holding
            return new Dictionary<string, decimal>();
        }
    }

    public class RiskAssessment
    {
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Side { get; set; }
        public TradeType TradeType { get; set; }
        public RiskLevel PositionSizeRisk { get; set; }
        public RiskLevel SectorConcentrationRisk { get; set; }
        public RiskLevel DailyLossRisk { get; set; }
        public RiskLevel VolatilityRisk { get; set; }
        public RiskLevel LiquidityRisk { get; set; }
        public double OverallRiskScore { get; set; }
        public bool IsTradeAllowed { get; set; }
        public string RiskReason { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class RiskMetrics
    {
        public decimal TotalPortfolioValue { get; set; }
        public decimal CashPercentage { get; set; }
        public int NumberOfPositions { get; set; }
        public decimal LargestPositionPercentage { get; set; }
        public decimal CurrentDrawdown { get; set; }
        public decimal DailyPnL { get; set; }
        public Dictionary<string, decimal> SectorExposures { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum RiskLevel
    {
        Low,
        Medium,
        High
    }

    public class RiskConfig
    {
        public double MaxRiskScore { get; set; } = 0.7;
        public decimal MaxPositionSizePercentage { get; set; } = 0.05m; // 5%
        public decimal MaxPositionSizeAbsolute { get; set; } = 50000m;
        public decimal MaxSectorExposurePercentage { get; set; } = 0.25m; // 25%
        public decimal MaxDrawdownAmount { get; set; } = 10000m;
        public decimal PositionSizePercentage { get; set; } = 0.02m; // 2%
        
        // Risk level thresholds
        public decimal LowRiskPositionPercentage { get; set; } = 0.02m;
        public decimal MediumRiskPositionPercentage { get; set; } = 0.05m;
        public decimal LowRiskSectorPercentage { get; set; } = 0.15m;
        public decimal MediumRiskSectorPercentage { get; set; } = 0.25m;
        public decimal LowRiskDailyLossAmount { get; set; } = 1000m;
        public decimal MediumRiskDailyLossAmount { get; set; } = 5000m;
        public decimal MaxDailyLossPercentage { get; set; } = 0.02m; // 2%
        
        // Weights for overall risk calculation
        public double PositionSizeWeight { get; set; } = 0.3;
        public double SectorConcentrationWeight { get; set; } = 0.2;
        public double DailyLossWeight { get; set; } = 0.25;
        public double VolatilityWeight { get; set; } = 0.15;
        public double LiquidityWeight { get; set; } = 0.1;
    }
} 