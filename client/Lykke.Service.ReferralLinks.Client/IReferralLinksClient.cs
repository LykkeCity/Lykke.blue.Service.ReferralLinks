
using Lykke.Blue.Service.ReferralLinks.Client.AutorestClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Blue.Service.ReferralLinks.Client
{
    public interface IReferralLinksClient
    {
        //Task<CreateReferralLinkResponse> Create(CreateReferralLinkRequest request);
        Task<GetReferralLinkResponse> Get(string id);
        Task<IEnumerable<GetReferralLinkResponse>> GetReferralLinksBySenderIdAndOrStatus(string senderClientId, string state);
        Task UpdateState(string id, string state);
        Task<GetReferralLinksStatisticsBySenderIdResponse> GetReferralLinksStatisticsBySenderId(string senderClientId);
        Task SetUrl(string id, string url);
        Task<string> ClaimGiftCoins(ClaimGiftCoinsRequest request);
        Task<string> RequestReferralLink(RequestReferralLinkRequest request);
    }
}
