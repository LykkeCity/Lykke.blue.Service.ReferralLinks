using Core.BitCoin.BitcoinApi.Models;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi
{
    public interface IBitcoinApiClient
    {   
        //offchain
        Task<OffchainResponse> OffchainTransferAsync(OffchainTransferData data);
        Task<OffchainClosingResponse> CreateChannelAsync(CreateChannelData data);
        Task<OffchainResponse> CreateHubCommitment(CreateHubComitmentData data);
        Task<OffchainResponse> Finalize(FinalizeData data);
        Task<OffchainBaseResponse> CloseChannel(CloseChannelData data);
    }
}
