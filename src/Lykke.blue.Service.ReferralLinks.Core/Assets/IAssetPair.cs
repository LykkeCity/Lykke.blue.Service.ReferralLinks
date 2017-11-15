using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.blue.Service.ReferralLinks.Core.Assets
{
    public interface IAssetPair
    {
        string Id { get; }
        string Name { get; }
        string BaseAssetId { get; }
        string QuotingAssetId { get; }
        int Accuracy { get; }
        int InvertedAccuracy { get; }
        string Source { get; }
        string Source2 { get; }
        bool IsDisabled { get; }
    }
}
