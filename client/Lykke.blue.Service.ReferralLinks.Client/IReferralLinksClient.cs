
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global
// interface intended for external usage

namespace Lykke.blue.Service.ReferralLinks.Client
{
    public interface IReferralLinksClient
    {
        Task<object> GetReferralLink(string id);
        Task<object> GetReferralLinkByUrl(string url);
        Task<object> GetReferralLinksStatisticsBySenderId(string senderClientId);
        Task<object> RequestGiftCoinsReferralLink(GiftCoinRequest request);
        Task<object> GroupGenerateGiftCoinLinks(GiftCoinRequestGroup request);
        Task<object> RequestInvitationReferralLink(InvitationReferralLinkRequest request);
        Task<object> ClaimGiftCoins(string refLinkId, ClaimReferralLinkRequest request);
        Task<object> ClaimInvitationLink(string refLinkId, ClaimReferralLinkRequest request);
        Task<object> GetGiftCoinReferralLinks(string senderClientId);
        
    }
}
