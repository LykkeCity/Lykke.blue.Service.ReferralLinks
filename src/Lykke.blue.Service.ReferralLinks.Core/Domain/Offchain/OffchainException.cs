using Core.BitCoin.BitcoinApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public class OffchainException : Exception
    {
        public ErrorCode Type { get; }

        public string OffchainExceptionMessage { get; }
        public string OffchainExceptionCode { get; }


        public string AssetId { get; }

        public bool ShouldCheckAsset { get; }

        public OffchainException(ErrorCode type)
        {
            Type = type;
        }

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
