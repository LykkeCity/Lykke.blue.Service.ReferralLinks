using System;

namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models
{
    public class OffchainResponse : OffchainBaseResponse
    {
        public string Transaction { get; set; }
        public Guid? TransferId { get; set; }
    }
}
