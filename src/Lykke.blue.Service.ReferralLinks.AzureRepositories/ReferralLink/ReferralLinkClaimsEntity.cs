using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.ReferralLink
{
    public class ReferralLinkClaimEntity : TableEntity, IReferralLinkClaim
    {
        public string Id => RowKey;
        public string ReferralLinkId { get; set; }
        public string RecipientClientId { get; set; }
        public bool ShouldReceiveReward { get; set; }
        public string RecipientTransactionId { get; set; }
        public bool IsNewClient { get; set; }

        
    }
}
