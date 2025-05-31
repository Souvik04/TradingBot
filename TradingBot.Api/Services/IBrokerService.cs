using System.Threading.Tasks;
using TradingBot.Api.Models;

namespace TradingBot.Api.Services
{
    public interface IBrokerService
    {
        Task<string> PlaceOrderAsync(string symbol, int quantity, decimal price, string side, string orderType);
        Task<string> GetOrderStatusAsync(string orderId);
        Task<Holding[]> GetHoldingsAsync();
    }
}