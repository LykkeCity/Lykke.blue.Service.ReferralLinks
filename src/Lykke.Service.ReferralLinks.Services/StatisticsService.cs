using Lykke.Service.ReferralLinks.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using System.Threading.Tasks;
using System.Linq;
using Lykke.Service.ReferralLinks.Services.Domain;

namespace Lykke.Service.ReferralLinks.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IReferralLinkRepository _referralLinkRepository;
        private readonly IReferralLinkClaimsRepository _referralLinkClaimsRepository;

        public StatisticsService(IReferralLinkRepository referralLinkRepository, 
                                 IReferralLinkClaimsRepository referralLinkClaimsRepository)
        {
            _referralLinkRepository = referralLinkRepository;
            _referralLinkClaimsRepository = referralLinkClaimsRepository;
        }


        public async Task<IReferralLinksStatistics> GetStatistics(string senderClientId) 
        {
            var statistics = new ReferralLinksStatistics();

            var referralLinksForSender = await _referralLinkRepository.GetReferralLinksBySenderId(senderClientId);

            var invitationLink = referralLinksForSender.FirstOrDefault(r => r.Type == ReferralLinkType.Invitation.ToString());

            if (invitationLink != null)
            {
                statistics.NumberOfInvitationLinksSent = 1;
                var claims = await _referralLinkClaimsRepository.GetClaimsForRefLinks(new [] { invitationLink.Id });
                statistics.NumberOfInvitationLinksAccepted = claims.Count();
            }

            statistics.NumberOfGiftLinksSent = referralLinksForSender.Where(r => r.Type == ReferralLinkType.GiftCoins.ToString()).Count();

            statistics.AmountOfGiftCoinsDistributed = referralLinksForSender
                .Where(x => x.Type == ReferralLinkType.GiftCoins.ToString() && x.State == ReferralLinkState.Claimed.ToString())
                .Sum(x => x.Amount);

            statistics.NumberOfNewUsersBroughtIn = (await _referralLinkClaimsRepository.GetClaimsForRefLinks(referralLinksForSender.Select(r => r.Id))).Where(r => r.IsNewClient).Count();

            return statistics;
        }
    }
}
