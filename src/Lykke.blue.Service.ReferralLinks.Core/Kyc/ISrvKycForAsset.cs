using System.Threading.Tasks;
using Lykke.blue.Service.ReferralLinks.Core.Assets;

namespace Lykke.blue.Service.ReferralLinks.Core.Kyc
{
    public interface ISrvKycForAsset
    {
        Task<bool> IsKycNeeded(string clientId, string assetId);
        Task<bool?> CanSkipKyc(string clientId, string assetId, IAssetPair assetPair, decimal volume);
    }
}
