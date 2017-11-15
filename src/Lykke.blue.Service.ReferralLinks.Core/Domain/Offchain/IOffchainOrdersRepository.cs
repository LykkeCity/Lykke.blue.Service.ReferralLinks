using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

    public interface IOffchainOrdersRepository
    {
        Task<IOffchainOrder> GetOrder(string id);
        Task<IOffchainOrder> CreateOrder(string clientId, string asset, string assetPair, decimal volume, decimal reservedVolume, bool straight);
        Task<IOffchainOrder> CreateLimitOrder(string clientId, string asset, string assetPair, decimal volume, decimal reservedVolume, bool straight, decimal price);
        Task UpdatePrice(string orderId, decimal price);
    }
}
