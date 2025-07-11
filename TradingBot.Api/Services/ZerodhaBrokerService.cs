using System;
using System.Threading.Tasks;
using TradingBot.Api.Models;
using KiteConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;

namespace TradingBot.Api.Services
{
    public class ZerodhaBrokerService : IBrokerService
    {
        private readonly KiteConnect _kite;
        private readonly ILogger<ZerodhaBrokerService> _logger;
        private readonly BrokerConfig _config;

        public ZerodhaBrokerService(IOptions<BrokerConfig> config, ILogger<ZerodhaBrokerService> logger)
        {
            _config = config.Value;
            _logger = logger;
            
            _kite = new KiteConnect(_config.ApiKey);
            
            // Set access token if available
            if (!string.IsNullOrEmpty(_config.AccessToken))
            {
                _kite.SetAccessToken(_config.AccessToken);
            }
        }

        public async Task<string> PlaceOrderAsync(string symbol, int quantity, decimal price, string side, string orderType)
        {
            try
            {
                _logger.LogInformation($"Placing {orderType} order: {side} {quantity} {symbol} @ {price}");

                // Validate order parameters
                if (quantity <= 0)
                    throw new ArgumentException("Quantity must be positive");

                if (price <= 0)
                    throw new ArgumentException("Price must be positive");

                // Map order type to Zerodha format
                var kiteOrderType = MapOrderType(orderType);
                var kiteSide = MapSide(side);

                // Create order parameters
                var orderParams = new OrderParams
                {
                    TradingSymbol = symbol,
                    Quantity = quantity,
                    Price = (double)price,
                    Product = _config.ProductType, // CNC, MIS, NRML
                    OrderType = kiteOrderType,
                    Side = kiteSide,
                    Validity = "DAY"
                };

                // Add stop loss if configured
                if (_config.EnableStopLoss && side.ToLower() == "buy")
                {
                    orderParams.StopLoss = (double)(price * (1 - _config.StopLossPercentage));
                }

                // Place order
                var response = await Task.Run(() => _kite.PlaceOrder(orderParams, _config.Variety));

                _logger.LogInformation($"Order placed successfully. Order ID: {response}");

                // Store order details for tracking
                await StoreOrderDetails(response, symbol, quantity, price, side, orderType);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to place order: {symbol} {side} {quantity} @ {price}");
                throw;
            }
        }

        public async Task<string> GetOrderStatusAsync(string orderId)
        {
            try
            {
                var orders = await Task.Run(() => _kite.GetOrders());
                var order = orders.FirstOrDefault(o => o.OrderId == orderId);
                
                return order?.Status ?? "UNKNOWN";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get order status for {orderId}");
                throw;
            }
        }

        public async Task<Holding[]> GetHoldingsAsync()
        {
            try
            {
                var holdings = await Task.Run(() => _kite.GetHoldings());
                
                return holdings.Select(h => new Holding
                {
                    Symbol = h.TradingSymbol,
                    Quantity = h.Quantity,
                    AveragePrice = (decimal)h.AveragePrice,
                    CurrentPrice = (decimal)h.LastPrice,
                    CurrentValue = (decimal)(h.Quantity * h.LastPrice),
                    PnL = (decimal)h.Pnl
                }).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get holdings");
                throw;
            }
        }

        public async Task<decimal> GetCashBalanceAsync()
        {
            try
            {
                var margins = await Task.Run(() => _kite.GetMargins("equity"));
                return (decimal)margins.Net;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cash balance");
                throw;
            }
        }

        private string MapOrderType(string orderType)
        {
            return orderType.ToLower() switch
            {
                "market" => "MARKET",
                "limit" => "LIMIT",
                "stop" => "SL",
                "stoplimit" => "SL-M",
                _ => "MARKET"
            };
        }

        private string MapSide(string side)
        {
            return side.ToLower() switch
            {
                "buy" => "BUY",
                "sell" => "SELL",
                _ => throw new ArgumentException($"Invalid side: {side}")
            };
        }

        private async Task StoreOrderDetails(string orderId, string symbol, int quantity, decimal price, string side, string orderType)
        {
            // Store order details in database for audit trail
            // This would typically go to your audit log table
            _logger.LogInformation($"Order stored: {orderId} - {symbol} {side} {quantity} @ {price} ({orderType})");
        }
    }

    public class BrokerConfig
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string AccessToken { get; set; }
        public string ProductType { get; set; } = "CNC"; // CNC, MIS, NRML
        public string Variety { get; set; } = "regular";
        public bool EnableStopLoss { get; set; } = true;
        public decimal StopLossPercentage { get; set; } = 0.02m; // 2%
    }
} 