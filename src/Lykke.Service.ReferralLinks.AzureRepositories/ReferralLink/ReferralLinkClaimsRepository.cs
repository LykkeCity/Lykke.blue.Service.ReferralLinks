using AutoMapper;
using AzureStorage;
using Lykke.Service.ReferralLinks.AzureRepositories.DTOs;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.AzureRepositories.ReferralLink
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

        public async Task<IReferralLinkClaim> Get(string id)
        {
            var entity = await _referralLinkClaimsTable.GetDataAsync(GetPartitionKey(), GetRowKey(id));

            return Mapper.Map<ReferralLinkClaimsDto>(entity);
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
