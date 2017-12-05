using System;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Exceptions
{
    public class TradeException : Exception
    {
        public TradeExceptionType Type { get; }

        public TradeException(TradeExceptionType type)
        {
            Type = type;
        }
    }
}
