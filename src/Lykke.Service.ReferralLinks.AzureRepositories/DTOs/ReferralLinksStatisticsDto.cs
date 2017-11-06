using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.AzureRepositories.DTOs
{
    public class ReferralLinksStatisticsDto : IReferralLinksStatistics
    {
        public int NumberOfInvitationsSent { get; set; }

        public int NumberOfInvitationAccepted { get; set; }

        public decimal AmountOfCoinsDistributed { get; set; }

        public int NumberOfNewUsersBroughtIn { get; set; }
    }
}
