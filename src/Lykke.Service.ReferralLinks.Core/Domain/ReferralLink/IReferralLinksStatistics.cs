using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Core.Domain.ReferralLink
{
    public interface IReferralLinksStatistics
    {
        int NumberOfInvitationsSent { get; }
        int NumberOfInvitationAccepted { get; }
        double AmountOfCoinsDistributed { get; }
        int NumberOfNewUsersBroughtIn { get; }
    }
}
