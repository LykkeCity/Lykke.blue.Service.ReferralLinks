using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Kyc
{
    public interface ISrvKycForAsset
    {
        Task<bool> IsKycNeeded(string clientId, string assetId);
    }
}
