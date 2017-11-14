using AzureStorage;
using Lykke.Blue.Service.ReferralLinks.Core.Domain.Offchain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Blue.Service.ReferralLinks.AzureRepositories.Offchain
{
    public class OffchainOrder : BaseEntity, IOffchainOrder
    {
        public string Id => RowKey;

        public string OrderId { get; set; }
        public string ClientId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Volume { get; set; }
        public decimal ReservedVolume { get; set; }
        public string AssetPair { get; set; }
        public string Asset { get; set; }
        public bool Straight { get; set; }
        public decimal Price { get; set; }
        public bool IsLimit { get; set; }


        public static string GeneratePartitionKey()
        {
            return "Order";
        }

        public static OffchainOrder Create(string clientId, string asset, string assetPair, decimal volume, decimal reservedVolume,
            bool straight, decimal price = 0)
        {
            var id = Guid.NewGuid().ToString();
            return new OffchainOrder
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = id,
                OrderId = id,
                ClientId = clientId,
                CreatedAt = DateTime.UtcNow,
                Volume = volume,
                ReservedVolume = reservedVolume,
                AssetPair = assetPair,
                Asset = asset,
                Straight = straight,
                Price = price,
                IsLimit = price > 0
            };
        }
    }

    public class OffchainOrderRepository : IOffchainOrdersRepository
    {
        private readonly INoSQLTableStorage<OffchainOrder> _storage;

        public OffchainOrderRepository(INoSQLTableStorage<OffchainOrder> storage)
        {
            _storage = storage;
        }

        public async Task<IOffchainOrder> GetOrder(string id)
        {
            return await _storage.GetDataAsync(OffchainOrder.GeneratePartitionKey(), id);
        }

        public async Task<IOffchainOrder> CreateOrder(string clientId, string asset, string assetPair, decimal volume, decimal reservedVolume, bool straight)
        {
            var entity = OffchainOrder.Create(clientId, asset, assetPair, volume, reservedVolume, straight);
            await _storage.InsertAsync(entity);
            return entity;
        }

        public async Task<IOffchainOrder> CreateLimitOrder(string clientId, string asset, string assetPair, decimal volume, decimal reservedVolume, bool straight, decimal price)
        {
            var entity = OffchainOrder.Create(clientId, asset, assetPair, volume, reservedVolume, straight, price);
            await _storage.InsertAsync(entity);
            return entity;
        }

        public Task UpdatePrice(string orderId, decimal price)
        {
            return _storage.ReplaceAsync(OffchainOrder.GeneratePartitionKey(), orderId, order =>
            {
                order.Price = price;
                return order;
            });
        }
    }
}
