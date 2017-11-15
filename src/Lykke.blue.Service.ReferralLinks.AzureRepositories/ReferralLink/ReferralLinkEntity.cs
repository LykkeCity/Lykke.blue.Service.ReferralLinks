using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

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

        public static IEqualityComparer<ReferralLinkEntity> ComparerById { get; } = new EqualityComparerById();

        private class EqualityComparerById : IEqualityComparer<ReferralLinkEntity>
        {
            public bool Equals(ReferralLinkEntity x, ReferralLinkEntity y)
            {
                if (x == y)
                    return true;
                if (x == null || y == null)
                    return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(ReferralLinkEntity obj)
            {
                if (obj?.Id == null)
                    return 0;
                return obj.Id.GetHashCode();
            }
        }
    }
}
