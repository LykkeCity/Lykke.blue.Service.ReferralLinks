using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Core.Domain.Exceptions
{
    public enum TradeExceptionType
    {
        LeadToNegativeSpread = 1,
        PriceGapTooHigh = 2
    }

    public class TradeException : Exception
    {
        public TradeExceptionType Type { get; }

        public TradeException(TradeExceptionType type)
        {
            Type = type;
        }
    }
}
