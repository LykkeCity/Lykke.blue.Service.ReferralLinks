using Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models;
using System;

namespace Core.BitCoin.BitcoinApi.Models
{

    public class OffchainBaseResponse
    {
        public ErrorResponse Error { get; set; }

        public bool HasError => Error != null;

        public string TxHash { get; set; }
    }

    public class OffchainResponse : OffchainBaseResponse
    {
        public string Transaction { get; set; }
        public Guid? TransferId { get; set; }
    }

    public class OffchainClosingResponse : OffchainResponse
    {
        public bool ChannelClosing { get; set; }
    }

   

   

    public class OffchainChannelBalance
    {
        public string Multisig { get; set; }
        public decimal ClientAmount { get; set; }
        public decimal HubAmount { get; set; }
        public string Hash { get; set; }
        public bool Actual { get; set; }
    }
}
