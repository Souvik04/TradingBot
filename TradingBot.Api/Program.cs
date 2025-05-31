
using Microsoft.EntityFrameworkCore;
using TradingBot.Api.Data;
using TradingBot.Api.Services;

namespace TradingBot.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Bind TradeSettings from config
            builder.Services.Configure<TradingBot.Api.Models.TradeSettings>(
                builder.Configuration.GetSection("TradeSettings"));

            // Register services
            builder.Services.AddSingleton<IPortfolioManagementService, PortfolioManagementService>();
            builder.Services.AddSingleton<IBrokerService, MockBrokerService>();
            builder.Services.AddHostedService<DailyStatsResetService>();
            builder.Services.AddScoped<IPortfolioManagementService, PortfolioManagementService>();
            builder.Services.AddDbContext<TradingBotDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("TradingBotDb")));

            // Add services to the container.
            builder.Services.AddHttpClient();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
