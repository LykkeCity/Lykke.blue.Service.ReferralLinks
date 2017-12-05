using System;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainOrder
    {
        string Id { get; }
        string OrderId { get; }
        string ClientId { get; set; }
        DateTime CreatedAt { get; set; }
        decimal Volume { get; set; }
        decimal ReservedVolume { get; set; }
        string AssetPair { get; set; }
        string Asset { get; set; }
        bool Straight { get; set; }
        decimal Price { get; set; }
        bool IsLimit { get; set; }
    }
}