﻿using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Services
{
    public interface IReferralLinksService
    {
        Task<IReferralLink> CreateInvitationLink(InvitationReferralLinkRequest referralLinkRequest);
        Task<IReferralLink> CreateGiftCoinsLink(GiftCoinsReferralLinkRequest referralLinkRequest);
        Task<IReferralLink> Get(string id);
        Task<IReferralLink> UpdateAsync(IReferralLink referralLink);
        Task<IReferralLink> GetReferralLinkById(string id);
        Task<IReferralLink> GetReferralLinkByUrl(string url);
        bool InvitationLinkForSenderIdExists(string senderClientId);
        Task CheckForExpiredGiftCoinLink();
        IEnumerable<IReferralLink> GetInvitationLinksForSenderId(string senderClientId);
    }
}
