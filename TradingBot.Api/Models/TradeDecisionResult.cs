namespace TradingBot.Api.Models
{
    public class TradeDecisionResult
    {
        public bool IsAllowed { get; set; }
        public string Reason { get; set; }
        public string SuggestedAction { get; set; }

        public static TradeDecisionResult Allowed() => new TradeDecisionResult { IsAllowed = true };
        public static TradeDecisionResult Denied(string reason, string suggestion = null)
            => new TradeDecisionResult { IsAllowed = false, Reason = reason, SuggestedAction = suggestion };
    }
}