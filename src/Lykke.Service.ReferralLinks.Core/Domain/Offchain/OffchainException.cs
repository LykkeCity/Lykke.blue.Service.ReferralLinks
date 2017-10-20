using Core.BitCoin.BitcoinApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ReferralLinks.Core.Domain.Offchain
{
    public class OffchainException : Exception
    {
        public ErrorCode Type { get; }

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
    }
}
