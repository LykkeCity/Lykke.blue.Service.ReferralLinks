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
        private readonly INoSQLTableStorage<ReferralLinkClaimsEntity> _referralLinkClaimsTable;

        public ReferralLinkClaimsRepository(INoSQLTableStorage<ReferralLinkClaimsEntity> referralLinkClaimsTable)
        {
            _referralLinkClaimsTable = referralLinkClaimsTable;
        }

        public static string GetPartitionKey() => "ReferallLinkClaims";

        public static string GetRowKey(string id)
        {
            return String.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        }

        public async Task<IReferralLinkClaims> Create(IReferralLinkClaims referralLinkClaims)
        {
            var entity = Mapper.Map<ReferralLinkClaimsEntity>(referralLinkClaims);

            entity.PartitionKey = GetPartitionKey();
            entity.RowKey = GetRowKey(referralLinkClaims.ReferralLinkId);

            await _referralLinkClaimsTable.InsertAsync(entity);

            return Mapper.Map<ReferralLinkClaimsDto>(entity);
        }

        public async Task Delete(string id)
        {
            await _referralLinkClaimsTable.DeleteAsync(GetPartitionKey(), GetRowKey(id));
        }

        public async Task<IReferralLinkClaims> Get(string id)
        {
            var entity = await _referralLinkClaimsTable.GetDataAsync(GetPartitionKey(), GetRowKey(id));

            return Mapper.Map<ReferralLinkClaimsDto>(entity);
        }

        public async Task<IReferralLinkClaims> Update(IReferralLinkClaims referralLinkClaims)
        {
            var result = await _referralLinkClaimsTable.MergeAsync(GetPartitionKey(), GetRowKey(referralLinkClaims.ReferralLinkId), x =>
            {
                Mapper.Map(referralLinkClaims, x);

                return x;
            });

            return Mapper.Map<ReferralLinkClaimsDto>(result);
        }
    }
}
