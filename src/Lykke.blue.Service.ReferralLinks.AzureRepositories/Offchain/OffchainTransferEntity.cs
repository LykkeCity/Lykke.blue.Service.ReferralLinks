// ReSharper disable ClassNeverInstantiated.Global
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using System;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Offchain
{
    public class OffchainTransferEntity : BaseEntity, IOffchainTransfer
    {
        public string Id => RowKey;
        public string ClientId { get; private set; }
        public string AssetId { get; private set; }
        public decimal Amount { get; private set; }
        public bool Completed { get; set; }
        public DateTime CreatedDt { get; set; }
        public string ExternalTransferId { get; set; }
        public OffchainTransferType Type { get; private set; }
        public bool ChannelClosing { get; set; }
        public bool Onchain { get; set; }
        public string BlockchainHash { get; set; }

        public class ByCommon
        {
            public static string GeneratePartitionKey()
            {
                return "OffchainTransfer";
            }

            public static OffchainTransferEntity Create(string id, string clientId, string assetId, decimal amount, OffchainTransferType type, string externalTransferId,
                bool channelClosing = false, bool onchain = false)
            {
                return new OffchainTransferEntity
                {
                    PartitionKey = GeneratePartitionKey(),
                    RowKey = id,
                    AssetId = assetId,
                    Amount = amount,
                    ClientId = clientId,
                    CreatedDt = DateTime.UtcNow,
                    ExternalTransferId = externalTransferId,
                    Type = type,
                    ChannelClosing = channelClosing,
                    Onchain = onchain
                };
            }
        }
    }
}
