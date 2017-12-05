using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.blue.Service.ReferralLinks.AzureRepositories.DTOs;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.ReferralLink
{
    public class ReferralLinkClaimsRepository : IReferralLinkClaimsRepository
    {
        private readonly INoSQLTableStorage<ReferralLinkClaimEntity> _referralLinkClaimsTable;

        public ReferralLinkClaimsRepository(INoSQLTableStorage<ReferralLinkClaimEntity> referralLinkClaimsTable)
        {
            _referralLinkClaimsTable = referralLinkClaimsTable;
        }

        public static string GetPartitionKey() => "ReferallLinkClaims";

        public static string GetRowKey(string id)
        {
            return String.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        }

        public async Task<IReferralLinkClaim> Create(IReferralLinkClaim referralLinkClaim)
        {
            var entity = Mapper.Map<ReferralLinkClaimEntity>(referralLinkClaim);

            entity.PartitionKey = GetPartitionKey();
            entity.RowKey = GetRowKey(referralLinkClaim.Id);

            await _referralLinkClaimsTable.InsertAsync(entity);

            return Mapper.Map<ReferralLinkClaimsDto>(entity);
        }

        public async Task Delete(string id)
        {
            await _referralLinkClaimsTable.DeleteAsync(GetPartitionKey(), GetRowKey(id));
        }

        public async Task<IEnumerable<IReferralLinkClaim>> GetClaimsForRefLinks(IEnumerable<string> refLinkIds)
        {
            //var claims = await _referralLinkClaimsTable.GetDataAsync(GetPartitionKey(), (link) => link.ReferralLinkId == refLinkId);
            var claims = await _referralLinkClaimsTable.GetDataAsync((link) => refLinkIds.Contains(link.ReferralLinkId));
            return claims; 
        }

        public async Task<IReferralLinkClaim> Update(IReferralLinkClaim referralLinkClaims)
        {
            var result = await _referralLinkClaimsTable.MergeAsync(GetPartitionKey(), GetRowKey(referralLinkClaims.Id), x =>
            {
                Mapper.Map(referralLinkClaims, x);

                return x;
            });

            return Mapper.Map<ReferralLinkClaimsDto>(result);
        }
    }
}
