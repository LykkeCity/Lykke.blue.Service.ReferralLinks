// ReSharper disable ClassNeverInstantiated.Global
using Common;
using Lykke.blue.Service.ReferralLinks.Core.Kyc;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.Kyc.Abstractions.Services;
using System;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Services.Kyc
{
    public class SrvKycForAsset : ISrvKycForAsset
    {
        private readonly CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> _assets;
        private readonly IKycStatusService _kycStatusService;

        public SrvKycForAsset(
            CachedDataDictionary<string, Lykke.Service.Assets.Client.Models.Asset> assets,
            IKycStatusService kycStatusService)
        {
            _assets = assets;
            _kycStatusService = kycStatusService;
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
    }
}
