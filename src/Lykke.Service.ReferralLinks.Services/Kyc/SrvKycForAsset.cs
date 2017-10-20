using Common;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.ReferralLinks.Core.Assets;
using Lykke.Service.ReferralLinks.Core.Kyc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Services.Kyc
{
    public class SrvKycForAsset : ISrvKycForAsset
    {
        private readonly CachedDataDictionary<string, Asset> _assets;
        private readonly IKycStatusService _kycStatusService;
        private readonly ISkipKycRepository _skipKycRepository;

        public SrvKycForAsset(
            CachedDataDictionary<string, Asset> assets,
            IKycStatusService kycStatusService,
            ISkipKycRepository skipKycRepository)
        {
            _assets = assets;
            _kycStatusService = kycStatusService;
            _skipKycRepository = skipKycRepository;
        }

        public async Task<bool> IsKycNeeded(string clientId, string assetId)
        {
            if (string.IsNullOrEmpty(assetId))
                throw new ArgumentException(nameof(assetId));

            var asset = await _assets.GetItemAsync(assetId);

            if (asset == null)
                throw new ArgumentException(nameof(assetId));

            var userKycStatus = await _kycStatusService.GetKycStatusAsync(clientId);

            return asset.KycNeeded && !userKycStatus.IsKycOkOrReviewDone();
        }

        public async Task<bool?> CanSkipKyc(string clientId, string assetId, IAssetPair assetPair, decimal volume)
        {
            var allowedAssets = new[] { "BTC", "ETH", "SLR", "TIME" };
            var canSkipKyc = await _skipKycRepository.CanSkipKyc(clientId);

            if (!canSkipKyc)
                return null;

            return volume > 0 && allowedAssets.Contains(assetId, StringComparer.InvariantCultureIgnoreCase) ||
                   volume < 0 && !allowedAssets.Contains(assetId, StringComparer.InvariantCultureIgnoreCase) &&
                   (allowedAssets.Contains(assetPair.BaseAssetId, StringComparer.InvariantCultureIgnoreCase) || allowedAssets.Contains(assetPair.QuotingAssetId, StringComparer.InvariantCultureIgnoreCase));
        }
    }
}
