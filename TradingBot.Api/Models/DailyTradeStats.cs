using System;
using System.ComponentModel.DataAnnotations;

namespace TradingBot.Api.Models
{
    public class DailyTradeStats
    {
        [Key]
        public DateTime Date { get; set; } // Store date at midnight for stats grouping

        public decimal BuyAmount { get; set; }
        public decimal SellAmount { get; set; }
        public int TradeCount { get; set; }
    }
}