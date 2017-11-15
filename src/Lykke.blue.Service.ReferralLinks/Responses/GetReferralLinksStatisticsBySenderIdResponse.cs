using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.blue.Service.ReferralLinks.Responses
{
    public class GetReferralLinksStatisticsBySenderIdResponse : IReferralLinksStatistics
    {
        public int NumberOfInvitationLinksSent { get; set; }

        public int NumberOfInvitationLinksAccepted { get; set; }

        public int NumberOfGiftLinksSent { get; set; }

        public double AmountOfGiftCoinsDistributed { get; set; }

        public int NumberOfNewUsersBroughtIn { get; set; }
    }
}
