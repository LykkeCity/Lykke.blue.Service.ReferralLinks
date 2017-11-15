using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.blue.Service.ReferralLinks.Models.Offchain
{
    public class OffchainChannelProcessModel
    {
        public string TransferId { get; set; }
        public string ClientId { get; set; }
        public string SignedChannelTransaction { get; set; }
    }
}
