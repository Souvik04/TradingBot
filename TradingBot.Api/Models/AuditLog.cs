using System;
using System.ComponentModel.DataAnnotations;

namespace TradingBot.Api.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }

        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string TradeType { get; set; }
        public bool IsBuy { get; set; }
        public string Decision { get; set; } // "Allowed" or "Blocked"
        public string Reason { get; set; }
        public string User { get; set; } // Optional: for multi-user systems
    }
}