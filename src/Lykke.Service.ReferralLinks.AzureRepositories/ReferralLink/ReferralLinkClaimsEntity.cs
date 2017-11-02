using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.AzureRepositories.ReferralLink
{
    public class ReferralLinkClaimsEntity : TableEntity
    {
        public string ReferralLinkId { get; set; }
        public string ClientId { get; set; }
        public bool ShouldReceive { get; set; }
        public bool HasReceived { get; set; }
        public bool IsNewUser { get; set; }

        public static IEqualityComparer<ReferralLinkClaimsEntity> ComparerById { get; } = new EqualityComparerById();


        private class EqualityComparerById : IEqualityComparer<ReferralLinkClaimsEntity>
        {
            public bool Equals(ReferralLinkClaimsEntity x, ReferralLinkClaimsEntity y)
            {
                if (x == y)
                    return true;
                if (x == null || y == null)
                    return false;
                return x.ReferralLinkId == y.ReferralLinkId;
            }

            public int GetHashCode(ReferralLinkClaimsEntity obj)
            {
                if (obj?.ReferralLinkId == null)
                    return 0;
                return obj.ReferralLinkId.GetHashCode();
            }
        }
    }
}
