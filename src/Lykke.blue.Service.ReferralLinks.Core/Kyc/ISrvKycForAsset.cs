using Lykke.Blue.Service.ReferralLinks.Core.Assets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Blue.Service.ReferralLinks.Core.Kyc
{
    public interface ISrvKycForAsset
    {
        Task<bool> IsKycNeeded(string clientId, string assetId);
        Task<bool?> CanSkipKyc(string clientId, string assetId, IAssetPair assetPair, decimal volume);
    }
}
