using AutoMapper;
using AzureStorage;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.ReferralLink
{
    public class ReferralLinkRepository : IReferralLinkRepository
    {
        private readonly INoSQLTableStorage<ReferralLinkEntity> _referralLinkTable;
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
        private const string ReflinkPartitionKey = "ReferallLink";

        public ReferralLinkRepository(INoSQLTableStorage<ReferralLinkEntity> referralLinkTable)
        {
            _referralLinkTable = referralLinkTable;
        }

        private static string GetPartitionKey() => ReflinkPartitionKey;

        private static string GetRowKey(string id) => id;

        public async Task<IReferralLink> Create(IReferralLink referralLink)
        {
            var entity = Mapper.Map<ReferralLinkEntity>(referralLink);

            entity.PartitionKey = GetPartitionKey();
            entity.RowKey = GetRowKey(referralLink.Id);

            await _referralLinkTable.InsertAsync(entity);

            return entity;
        }

        public async Task CreateGroup(IEnumerable<IReferralLink> referralLinks)
        {
            var many = new List<ReferralLinkEntity>();

            foreach (var refLink in referralLinks)
            {
                var entity = Mapper.Map<ReferralLinkEntity>(refLink);

                entity.PartitionKey = GetPartitionKey();
                entity.RowKey = GetRowKey(refLink.Id);

                many.Add(entity);
            }
            
            await _referralLinkTable.InsertAsync(many);
        }

        public async Task<IReferralLink> Get(string id)
        {
            var entity = await _referralLinkTable.GetDataAsync(GetPartitionKey(), GetRowKey(id));
            return entity;
        }

        public async Task<IReferralLink> GetReferalLinkByUrl(string url)
        {
            var entities = await _referralLinkTable.GetDataAsync(
                GetPartitionKey(),
                x => x.Url == url);

            return entities.FirstOrDefault();
        }

        public async Task<IEnumerable<IReferralLink>> GetReferralLinksBySenderId(string senderClientId)
        {
            return await _referralLinkTable.GetDataAsync(
                GetPartitionKey(),
                x => x.SenderClientId == senderClientId
            );            
        }

        public async Task<IEnumerable<IReferralLink>> GetExpiredGiftCoinLinks()
        {
            return await _referralLinkTable.GetDataAsync(
                GetPartitionKey(), 
                x => x.Type == ReferralLinkType.GiftCoins.ToString() && x.ExpirationDate < DateTime.UtcNow && x.State == ReferralLinkState.Created.ToString()
                );           
        }

        public async Task<IReferralLink> UpdateAsync(IReferralLink referralLink)
        {
            var result = await _referralLinkTable.MergeAsync(GetPartitionKey(), GetRowKey(referralLink.Id), x =>
            {
                Mapper.Map(referralLink, x);

                return x;
            });

            return result;
        }

        /// <summary>
        /// Ensures reflink state is not stale at the moment of update. Throws excpetion if it does.
        /// Relies on ETag checks, performed by both AzureTableStorage db and MergeAsync itself. 
        /// MergeAsync will return null if recored is stale, due to the Etag check in the injected lamda.
        /// Code is also thread safe. 
        /// </summary>
        /// <param name="referralLink"></param>
        /// <returns></returns>
        public async Task<IReferralLink> UpdateAsyncWithETagCheck(IReferralLink referralLink)
        {
            await SemaphoreSlim.WaitAsync();
            try
            {
                var result = await _referralLinkTable.MergeAsync(GetPartitionKey(), GetRowKey(referralLink.Id), currentDbRecord =>
                {
                    if (referralLink.ETag != currentDbRecord.ETag)
                    {
                        return null;
                    }

                    Mapper.Map(referralLink, currentDbRecord);

                    return currentDbRecord;
                });

                if (result == null)
                {
                    throw new Exception("Stale record, you may try again.");
                }

                return result;
            }
            finally
            {
                SemaphoreSlim.Release();
            }
            
        }
    }
}
