
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global
// interface intended for external usage

namespace Lykke.blue.Service.ReferralLinks.Client
{
    public interface IReferralLinksClient
    {
        Task<object> GetReferralLink(string id);
        Task<object> GetReferralLinksStatisticsBySenderId(string senderClientId);
        Task<object> RequestGiftCoinsReferralLink(GiftCoinsReferralLinkRequest request);
        Task<object> RequestInvitationReferralLink(InvitationReferralLinkRequest request);
        Task<object> ClaimGiftCoins(string refLinkId, ClaimReferralLinkRequest request);
        Task<object> ClaimInvitationLink(string refLinkId, ClaimReferralLinkRequest request);

        Task<OffchainEncryptedKeyRespModel> GetChannelKey(string asset, string clientId);
        Task<object> TransferToLykkeWallet(TransferToLykkeWallet request);
        Task<object> ProcessChannel(OffchainChannelProcessModel request);
        Task<object> FinalizeGiftCoinLinkTransfer(OffchainFinalizeModel request);



    }
}
