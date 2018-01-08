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
    public class ReferralLinkClaimsRepository : IReferralLinkClaimsRepository
    {
        private readonly INoSQLTableStorage<ReferralLinkClaimEntity> _referralLinkClaimsTable;
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        public ReferralLinkClaimsRepository(INoSQLTableStorage<ReferralLinkClaimEntity> referralLinkClaimsTable)
        {
            _referralLinkClaimsTable = referralLinkClaimsTable;
        }

        private static string GetPartitionKey(IReferralLinkClaim referralLinkClaim) => referralLinkClaim.ReferralLinkId;

        private static string GetRowKey(string id)
        {
            return String.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        }

        public async Task<IReferralLinkClaim> Create(IReferralLinkClaim referralLinkClaim)
        {
            var entity = Mapper.Map<ReferralLinkClaimEntity>(referralLinkClaim);

            entity.PartitionKey = GetPartitionKey(referralLinkClaim);
            entity.RowKey = GetRowKey(referralLinkClaim.Id);

            await _referralLinkClaimsTable.InsertAsync(entity);

            return entity;
        }

        public async Task<IEnumerable<IReferralLinkClaim>> GetClaimsForRefLinks(IEnumerable<string> refLinkIds)
        {
            var claims = await _referralLinkClaimsTable.GetDataAsync(link => refLinkIds.Contains(link.ReferralLinkId));
            return claims; 
        }

        public async Task<IReferralLinkClaim> Update(IReferralLinkClaim referralLinkClaim)
        {
            var result = await _referralLinkClaimsTable.MergeAsync(GetPartitionKey(referralLinkClaim), GetRowKey(referralLinkClaim.Id), x =>
            {
                Mapper.Map(referralLinkClaim, x);

                return x;
            });

            return result;
        }
    }
}
