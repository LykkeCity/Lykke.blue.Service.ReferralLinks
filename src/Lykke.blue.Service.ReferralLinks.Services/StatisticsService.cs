// ReSharper disable ClassNeverInstantiated.Global
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Services.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Services
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

            var referralLinksForSender = (await _referralLinkRepository.GetReferralLinksBySenderId(senderClientId)).ToList();

            var invitationLinks = referralLinksForSender.Where(r => r.Type == ReferralLinkType.Invitation.ToString()).ToList();

            statistics.NumberOfInvitationLinksSent = invitationLinks.Count;
            var claims = (await _referralLinkClaimsRepository.GetClaimsForRefLinks(invitationLinks.Select(r=>r.Id))).Where(l => l.RecipientClientId != senderClientId);
            statistics.NumberOfInvitationLinksAccepted = claims.Count();        

            statistics.NumberOfGiftLinksSent = referralLinksForSender.Count(r => r.Type == ReferralLinkType.GiftCoins.ToString());
            statistics.AmountOfGiftCoinsDistributed = referralLinksForSender
                .Where(x => x.Type == ReferralLinkType.GiftCoins.ToString() && x.State == ReferralLinkState.Claimed.ToString())
                .Sum(x => x.Amount);

            statistics.NumberOfNewUsersBroughtIn = (await _referralLinkClaimsRepository.GetClaimsForRefLinks(referralLinksForSender.Select(r => r.Id))).Count(r => r.IsNewClient);

            return statistics;
        }
    }
}
