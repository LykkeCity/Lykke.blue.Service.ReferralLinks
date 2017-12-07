using AzureStorage;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.Offchain
{
    public class OffchainRequestRepository : IOffchainRequestRepository
    {
        private readonly INoSQLTableStorage<OffchainRequestEntity> _table;

        public OffchainRequestRepository(INoSQLTableStorage<OffchainRequestEntity> table)
        {
            _table = table;
        }

        public async Task<IOffchainRequest> CreateRequest(string transferId, string clientId, string assetId, RequestType type, OffchainTransferType transferType)
        {
            var id = Guid.NewGuid().ToString();

            var byClient = OffchainRequestEntity.ByClient.Create(id, transferId, clientId, assetId, type, transferType, null);
            await _table.InsertAsync(byClient);

            var byRecord = OffchainRequestEntity.ByRecord.Create(id, transferId, clientId, assetId, type, transferType, null);
            await _table.InsertAsync(byRecord);

            return byRecord;
        }

        public async Task<IEnumerable<IOffchainRequest>> GetRequestsForClient(string clientId)
        {
            return await _table.GetDataAsync(OffchainRequestEntity.ByClient.GeneratePartition(clientId));
        }
        
        public async Task Complete(string requestId)
        {
            var record = await _table.DeleteAsync(OffchainRequestEntity.ByRecord.Partition, requestId);

            await _table.DeleteAsync(OffchainRequestEntity.ByClient.GeneratePartition(record.ClientId), requestId);

            await _table.InsertOrReplaceAsync(OffchainRequestEntity.Archieved.Create(record));
        }
    }
}
