using System.Threading.Tasks;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using AzureStorage;
using System;
using AutoMapper;
using Lykke.Service.ReferralLinks.AzureRepositories.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.ReferralLinks.AzureRepositories.ReferralLink
{
    public class ReferralLinkRepository : IReferralLinkRepository
    {
        private readonly INoSQLTableStorage<ReferralLinkEntity> _referralLinkTable;

        public ReferralLinkRepository(INoSQLTableStorage<ReferralLinkEntity> referralLinkTable)
        {
            _referralLinkTable = referralLinkTable;
        }

        public static string GetPartitionKey() => "ReferallLink";

        public static string GetRowKey(string id)
        {
            return String.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        }

        public async Task<string> ClaimGiftCoins(string referralLinkId, bool newUser, string claimingClientId)
        {
            var entity = await _referralLinkTable.GetDataAsync(GetPartitionKey(), GetRowKey(referralLinkId));
            entity.IsNewUser = newUser;
            entity.ClaimingClientId = claimingClientId;

            if (entity.UrlExpirationDate < DateTime.UtcNow && entity.State != ReferralLinkState.Expired.ToString())
            {
                entity.State = ReferralLinkState.Expired.ToString();
            }

            await _referralLinkTable.InsertOrReplaceAsync(entity);

            return entity.State;
        }

        public async Task<IReferralLink> Create(IReferralLink referralLink)
        {
            var entity = Mapper.Map<ReferralLinkEntity>(referralLink);

            entity.PartitionKey = GetPartitionKey();
            entity.RowKey = GetRowKey(referralLink.Id);

            await _referralLinkTable.InsertAsync(entity);

            return Mapper.Map<ReferralLinkDto>(entity);
        }

        public async Task Delete(string id)
        {
            await _referralLinkTable.DeleteAsync(GetPartitionKey(), GetRowKey(id));
        }

        public async Task<IReferralLink> Get(string id)
        {
            var entity = await _referralLinkTable.GetDataAsync(GetPartitionKey(), GetRowKey(id));

            return Mapper.Map<ReferralLinkDto>(entity);
        }

        public async Task<IEnumerable<IReferralLink>> Get(string senderClientId, string state)
        {
            var entities = await _referralLinkTable.GetDataAsync(
                GetPartitionKey(),
                x => (String.IsNullOrEmpty(senderClientId) || x.SenderClientId == senderClientId) && (String.IsNullOrEmpty(state) || x.State == state)
            );

            return Mapper.Map<IEnumerable<ReferralLinkDto>>(entities);
        }

        public async Task<IReferralLinksStatistics> GetReferralLinksStatisticsBySenderId(string senderClientId)
        {
            var referralLinks = await _referralLinkTable.GetDataAsync(
                GetPartitionKey(),
                x => x.SenderClientId == senderClientId
            );

            var numberOfInvitationSent = referralLinks.Count(x => x.State == ReferralLinkState.SentToRecipient.ToString());
            var numberOfInvitationAccepted = referralLinks.Count(x => x.State == ReferralLinkState.Claimed.ToString());

            //TODO: Calculate amount of coins distributed
            var amountOfCoinsDistributed = 0;
            //TODO: Calculate amount of new users brought in to the Lykke platform
            var numberOfNewUsersBroughtIn = 0;

            return new ReferralLinksStatisticsDto
            {
                AmountOfCoinsDistributed = amountOfCoinsDistributed,
                NumberOfInvitationAccepted = numberOfInvitationAccepted,
                NumberOfInvitationsSent = numberOfInvitationSent,
                NumberOfNewUsersBroughtIn = numberOfNewUsersBroughtIn
            };
        }

        public async Task SetUrl(string id, string url)
        {
            var entity = await _referralLinkTable.GetDataAsync(GetPartitionKey(), GetRowKey(id));
            entity.Url = url;

            await _referralLinkTable.InsertOrReplaceAsync(entity);
        }

        public async Task<IReferralLink> Update(IReferralLink referralLink)
        {
            var result = await _referralLinkTable.MergeAsync(GetPartitionKey(), GetRowKey(referralLink.Id), x =>
            {
                Mapper.Map(referralLink, x);

                return x;
            });

            return Mapper.Map<ReferralLinkDto>(result);
        }

        public async Task UpdateState(string id, string state)
        {
            var entity = await _referralLinkTable.GetDataAsync(GetPartitionKey(), GetRowKey(id));
            entity.State = state;

            await _referralLinkTable.InsertOrReplaceAsync(entity);
        }
    }
}
