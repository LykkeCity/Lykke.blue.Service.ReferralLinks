using AutoMapper;
using AzureStorage;
using Lykke.blue.Service.ReferralLinks.AzureRepositories.DTOs;
using Lykke.blue.Service.ReferralLinks.Core.Domain.ReferralLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.AzureRepositories.ReferralLink
{
    public class ReferralLinkRepository : IReferralLinkRepository
    {
        private readonly INoSQLTableStorage<ReferralLinkEntity> _referralLinkTable;

        public ReferralLinkRepository(INoSQLTableStorage<ReferralLinkEntity> referralLinkTable)
        {
            _referralLinkTable = referralLinkTable;
        }

        private static string GetPartitionKey() => "ReferallLink";

        private static string GetRowKey(string id) => id;

        public async Task<IReferralLink> Create(IReferralLink referralLink)
        {
            var entity = Mapper.Map<ReferralLinkEntity>(referralLink);

            entity.PartitionKey = GetPartitionKey();
            entity.RowKey = GetRowKey(referralLink.Id);

            await _referralLinkTable.InsertAsync(entity);

            return Mapper.Map<ReferralLinkDto>(entity);
        }        

        public async Task<IReferralLink> Get(string id)
        {
            var entity = await _referralLinkTable.GetDataAsync(GetPartitionKey(), GetRowKey(id));

            return Mapper.Map<ReferralLinkDto>(entity);
        }

        public async Task<IReferralLink> GetReferalLinkByUrl(string url)
        {
            var entities = await _referralLinkTable.GetDataAsync(
                GetPartitionKey(),
                x => x.Url == url);

            return Mapper.Map<ReferralLinkDto>(entities.FirstOrDefault());
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

            return Mapper.Map<ReferralLinkDto>(result);
        }

    }
}
