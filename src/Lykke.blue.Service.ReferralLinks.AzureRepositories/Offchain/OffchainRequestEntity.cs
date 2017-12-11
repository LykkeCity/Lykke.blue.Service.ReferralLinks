using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using System;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Offchain
{
    public class OffchainRequestEntity : BaseEntity, IOffchainRequest
    {
        public string RequestId => RowKey;
        public string TransferId { get; private set; }

        public string AssetId { get; private set; }

        public string ClientId { get; private set; }

        public RequestType Type { get; private set; }

        public DateTime? StartProcessing { get; private set; }

        public DateTime CreateDt { get; private set; }

        public int TryCount { get; private set; }

        public OffchainTransferType TransferType { get; private set; }

        public DateTime? ServerLock { get; private set; }

        public static class ByRecord
        {
            public static string Partition = "OffchainSignatureRequestEntity";

            public static OffchainRequestEntity Create(string id, string transferId, string clientId, string assetId, RequestType type, OffchainTransferType transferType, DateTime? serverLock)
            {
                var item = CreateNew(transferId, clientId, assetId, type, transferType, serverLock);

                item.PartitionKey = Partition;
                item.RowKey = id;

                return item;
            }
        }

        public static class ByClient
        {
            public static string GeneratePartition(string clientId)
            {
                return clientId;
            }

            public static OffchainRequestEntity Create(string id, string transferId, string clientId, string assetId, RequestType type, OffchainTransferType transferType, DateTime? serverLock)
            {
                var item = CreateNew(transferId, clientId, assetId, type, transferType, serverLock);

                item.PartitionKey = GeneratePartition(clientId);
                item.RowKey = id;

                return item;
            }
        }

        public static class Archieved
        {
            private static string GeneratePartition()
            {
                return "Archieved";
            }

            public static OffchainRequestEntity Create(IOffchainRequest request)
            {
                return new OffchainRequestEntity
                {
                    RowKey = request.RequestId,
                    PartitionKey = GeneratePartition(),
                    TransferId = request.TransferId,
                    ClientId = request.ClientId,
                    AssetId = request.AssetId,
                    Type = request.Type,
                    CreateDt = request.CreateDt == DateTime.MinValue ? DateTime.UtcNow : request.CreateDt,
                    TryCount = request.TryCount,
                    TransferType = request.TransferType,
                    ServerLock = request.ServerLock,
                    StartProcessing = request.StartProcessing
                };
            }
        }

        private static OffchainRequestEntity CreateNew(string transferId, string clientId, string assetId, RequestType type, OffchainTransferType transferType, DateTime? serverLock = null)
        {
            return new OffchainRequestEntity
            {
                TransferId = transferId,
                ClientId = clientId,
                AssetId = assetId,
                Type = type,
                CreateDt = DateTime.UtcNow,
                TransferType = transferType,
                ServerLock = serverLock
            };
        }
    }
}
