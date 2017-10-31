using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Lykke.Service.ReferralLinks.Requests
{
    public class RequestReferralLinkRequest : IReferralLink
    {
        //REMARK: We do not need to allow someone to set Id. Id is set automatically.
        [IgnoreDataMember]
        public string Id { get; }

        [IgnoreDataMember]
        public string Url { get; set; }

        [IgnoreDataMember]
        public DateTime ExpirationDate { get; set; }

        public string SenderClientId { get; set; }

        public string Asset { get; set; }

        [IgnoreDataMember]
        public bool? IsNewUser { get; set; }

        [IgnoreDataMember]
        public ReferralLinkState State { get; set; }

        public double Amount { get; set; }

        [IgnoreDataMember]
        public string ClaimingClientId { get; set; }
    }
}
