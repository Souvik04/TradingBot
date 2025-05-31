using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingBot.Api.Services;

namespace TradingBot.Api.Services
{
    public class DailyStatsResetService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyStatsResetService> _logger;
        private readonly TimeZoneInfo _istZone;

        public DailyStatsResetService(IServiceProvider serviceProvider, ILogger<DailyStatsResetService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _istZone);
                var nextMidnight = now.Date.AddDays(1);
                var delay = nextMidnight - now;
                _logger.LogInformation("DailyStatsResetService waiting {delay} until next reset at {nextMidnight}", delay, nextMidnight);

                await Task.Delay(delay, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var portfolioService = scope.ServiceProvider.GetRequiredService<IPortfolioManagementService>();
                portfolioService.ResetDailyLimits("system");
                _logger.LogInformation("Daily stats reset at midnight IST");
            }
        }
    }
}