using System;

namespace TradingBot.Api.Models
{
    [Flags]
    public enum TradeType
    {
        None = 0,
        Intraday = 1,
        Swing = 2,
        LongTerm = 4,
        Options = 8
    }
}