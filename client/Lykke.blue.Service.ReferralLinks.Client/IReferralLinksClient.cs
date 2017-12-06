
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Client
{
    //this client is intended for an external use
    public interface IReferralLinksClient
    {
        Task<object> GetReferralLink(string id);
        Task<object> GetReferralLinksStatisticsBySenderId(RefLinkStatisticsRequest request);
        Task<object> RequestGiftCoinsReferralLink(GiftCoinsReferralLinkRequest request);
        Task<object> RequestInvitationReferralLink(InvitationReferralLinkRequest request);
        Task<object> ClaimGiftCoins(ClaimReferralLinkRequest request);
        Task<object> ClaimInvitationLink(ClaimReferralLinkRequest request);

        Task<OffchainEncryptedKeyRespModel> GetChannelKey(OffchainGetChannelKeyRequest request);
        Task<object> TransferToLykkeWallet(TransferToLykkeWallet request);
        Task<object> ProcessChannel(OffchainChannelProcessModel request);
        Task<object> FinalizeRefLinkTransfer(OffchainFinalizeModel request);



    }
}
