using System;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using System.Runtime.Serialization;

namespace Lykke.Service.ReferralLinks.Responses
{
    public class RequestReferralLinkResponse
    {
        public string Id { get; set; }

        public string Url { get; set; }
    }
}
