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
        private const string GroupReflinkIdentificator = "Group";
        private const string ReflinkIdentificator = "ReferallLink";

        public ReferralLinkRepository(INoSQLTableStorage<ReferralLinkEntity> referralLinkTable)
        {
            _referralLinkTable = referralLinkTable;
        }

        private static string GetPartitionKey() => $"{ReflinkIdentificator}";

        private static string GetPartitionKeyGroupReferallLink(string guid) => $"{GroupReflinkIdentificator}-{guid}";
        
        private static string GetRowKey(string id) => id;

        public async Task<IReferralLink> Create(IReferralLink referralLink)
        {
            var entity = Mapper.Map<ReferralLinkEntity>(referralLink);

            entity.PartitionKey = GetPartitionKey();
            entity.RowKey = GetRowKey(referralLink.Id);

            await _referralLinkTable.InsertAsync(entity);

            return entity;
        }

        public async Task<string> CreateGroup(IEnumerable<IReferralLink> referralLinks)
        {
            var many = new List<ReferralLinkEntity>();
            var massReferallLinkGuid = Guid.NewGuid().ToString();

            foreach (var refLink in referralLinks)
            {
                var entity = Mapper.Map<ReferralLinkEntity>(refLink);

                entity.PartitionKey = GetPartitionKeyGroupReferallLink(massReferallLinkGuid);
                entity.RowKey = GetRowKey(refLink.Id);

                many.Add(entity);
            }
            
            await _referralLinkTable.InsertAsync(many);

            return massReferallLinkGuid;
        }

        public async Task<IReferralLink> Get(string id)
        {
            var entity = await _referralLinkTable.GetDataAsync(GetPartitionKey(), GetRowKey(id));
            return entity;
        }

        public async Task<IEnumerable<IReferralLink>> GetGroup(string groupId)
        {
            var groupOfRefLinks = await _referralLinkTable.GetDataAsync(GetPartitionKeyGroupReferallLink(groupId));
            return groupOfRefLinks;
        }

        public async Task<IEnumerable<IReferralLink>> GetGroupBySenderId(string senderId)
        {
            var groupOfRefLinks = (await _referralLinkTable.GetDataAsync(g=>g.SenderClientId == senderId)).Where(g=>g.PartitionKey.Contains(GroupReflinkIdentificator));
            return groupOfRefLinks;
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

        public bool IsInvitationLinkForSenderAlreadyCreated(string senderClientId)
        {
            var numberOfCreatedReflinks = _referralLinkTable.GetDataAsync(
                GetPartitionKey(),
                x => x.SenderClientId == senderClientId && x.Type == ReferralLinkType.Invitation.ToString()
            ).Result;

            return numberOfCreatedReflinks.Any();
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
