using Microsoft.EntityFrameworkCore;
using TradingBot.Api.Models;

namespace TradingBot.Api.Data
{
    public class TradingBotDbContext : DbContext
    {
        public TradingBotDbContext(DbContextOptions<TradingBotDbContext> options)
            : base(options) { }

        public DbSet<DailyTradeStats> DailyTradeStats { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}