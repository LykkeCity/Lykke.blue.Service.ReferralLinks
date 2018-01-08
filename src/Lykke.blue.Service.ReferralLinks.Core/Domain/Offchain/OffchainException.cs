using Lykke.Bitcoin.Api.Client.BitcoinApi.Models;
using System;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public class OffchainException : Exception
    {
        private ErrorCode Type { get; }

        public string OffchainExceptionMessage { get; }
        public string OffchainExceptionCode { get; }


        private string AssetId { get; }

        private bool ShouldCheckAsset { get; }

        public OffchainException(ErrorCode type, string assetId, bool shouldCheckAsset = true)
        {
            Type = type;
            AssetId = assetId;
            ShouldCheckAsset = shouldCheckAsset;
        }

        public OffchainException(ErrorCode type, string message, string offchainExceptionCode,  string assetId, bool shouldCheckAsset = true)
        {
            Type = type;
            AssetId = assetId;
            ShouldCheckAsset = shouldCheckAsset;
            OffchainExceptionMessage = message;
            OffchainExceptionCode = offchainExceptionCode;
        }
    }
}
