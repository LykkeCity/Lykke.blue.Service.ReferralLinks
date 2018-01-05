using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.ReferralLink
{
    public class ReferralLinkEntity : TableEntity, IReferralLink
    {
        public string Id => RowKey;
        public string Url { get; set; }
        public string SenderClientId { get; set; }
        public DateTime? ExpirationDate { get; set; }        
        public string Asset { get; set; }
        public double Amount { get; set; }
        public string SenderOffchainTransferId { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int ClaimsCount { get; set; }
    }
}
