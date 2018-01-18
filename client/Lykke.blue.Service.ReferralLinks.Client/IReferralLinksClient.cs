
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;
using Lykke.blue.Service.ReferralLinks.Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
// interface intended for external usage

namespace Lykke.blue.Service.ReferralLinks.Client
{
    public interface IReferralLinksClient
    {
        Task<ReferralLinkDto> GetReferralLinkAsync(string id);
        Task<ReferralLinkDto> GetReferralLinkByUrlAsync(string url);
        Task<ReferralLinksStatisticsDto> GetReferralLinksStatisticsBySenderIdAsync(string senderClientId);
        Task<Microsoft.AspNetCore.Mvc.ObjectResult> RequestGiftCoinsReferralLinkAsync(GiftCoinRequest request);
        Task<Microsoft.AspNetCore.Mvc.ObjectResult> GroupGenerateGiftCoinLinksAsync(GiftCoinRequestGroup request);
        Task<Microsoft.AspNetCore.Mvc.ObjectResult> RequestInvitationReferralLinkAsync(InvitationReferralLinkRequest request);
        Task<Microsoft.AspNetCore.Mvc.ObjectResult> ClaimGiftCoinsAsync(string refLinkId, ClaimReferralLinkRequest request);
        Task<Microsoft.AspNetCore.Mvc.ObjectResult> ClaimInvitationLinkAsync(string refLinkId, ClaimReferralLinkRequest request);
        Task<IEnumerable<ReferralLinkDto>> GetGiftCoinReferralLinksAsync(string senderClientId);
        
    }
}
