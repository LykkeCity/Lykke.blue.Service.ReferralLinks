using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.AzureRepositories.ReferralLink
{
    public class ReferralLinkClaimEntity : TableEntity, IReferralLinkClaim
    {
        public string Id => RowKey;
        public string ReferralLinkId { get; set; }
        public string RecipientClientId { get; set; }
        public bool ShouldReceiveReward { get; set; }
        public string RecipientTransactionId { get; set; }
        public bool IsNewClient { get; set; }

        public static IEqualityComparer<ReferralLinkClaimEntity> ComparerById { get; } = new EqualityComparerById();

        private class EqualityComparerById : IEqualityComparer<ReferralLinkClaimEntity>
        {
            public bool Equals(ReferralLinkClaimEntity x, ReferralLinkClaimEntity y)
            {
                if (x == y)
                    return true;
                if (x == null || y == null)
                    return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(ReferralLinkClaimEntity obj)
            {
                if (obj?.ReferralLinkId == null)
                    return 0;
                return obj.ReferralLinkId.GetHashCode();
            }
        }
    }
}
