using TradingBot.Api.Models;

namespace TradingBot.Api.Services
{
    public static class ConfigManager
    {
        private static Config _config = new();

        public static Config Get() => _config;
        public static void Save(Config config) => _config = config;
    }
}
