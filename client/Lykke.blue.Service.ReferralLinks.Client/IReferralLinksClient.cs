
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Client
{
    public interface IReferralLinksClient
    {
        Task<GetReferralLinkResponse> GetReferralLink(string id);
        Task<GetReferralLinksStatisticsBySenderIdResponse> GetReferralLinksStatisticsBySenderId(string senderClientId);
        Task<string> RequestGiftCoinsReferralLink(GiftCoinsReferralLinkRequest request);
        Task<string> RequestInvitationReferralLink(InvitationReferralLinkRequest request);
        Task<string> ClaimGiftCoins(ClaimReferralLinkRequest request);
        Task<string> ClaimInvitationLink(ClaimReferralLinkRequest request);

        Task<string> GetChannelKey(string asset, string clientId);
        Task<string> TransferToLykkeWallet(TransferToLykkeWallet request);
        Task<string> ProcessChannel(OffchainChannelProcessModel request);
        Task<string> FinalizeRefLinkTransfer(OffchainFinalizeModel request);



    }
}
