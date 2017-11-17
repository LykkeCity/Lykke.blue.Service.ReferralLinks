using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.blue.Service.ReferralLinks.Models.Offchain
{
    public class OffchainGetChannelKeyRequest
    {
        public string Asset { get; set; }
        public string ClientId { get; set; }
    }
}
