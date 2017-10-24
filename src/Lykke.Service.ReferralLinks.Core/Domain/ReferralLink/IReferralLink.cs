using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLink
    {
        string Id { get; }
        string Url { get; }
        DateTime UrlExpirationDate { get; }
        string ClientIdSender { get; }
        string RecipientEmail { get; }
        string AssetId { get; }
        RecipientType RecipientType { get; }
        ReferralLinkState State { get; }
    }
}
