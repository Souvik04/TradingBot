namespace TradingBot.Api.Models
{
    public class SignalEngineResponse
    {
        public string Prediction { get; set; }
        public double ConfidenceScore { get; set; }
        public string Reason { get; set; }
    }
}