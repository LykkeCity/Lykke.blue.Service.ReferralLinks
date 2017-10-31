using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Core.Domain.Offchain
{
    public class OffchainResultOrder
    {
        public string Id { get; set; }

        public DateTime DateTime { get; set; }

        public OrderType OrderType { get; set; }

        public double Volume { get; set; }

        public double TotalCost { get; set; }

        public double Price { get; set; }

        public string Asset { get; set; }

        public string AssetPair { get; set; }
    }
}
