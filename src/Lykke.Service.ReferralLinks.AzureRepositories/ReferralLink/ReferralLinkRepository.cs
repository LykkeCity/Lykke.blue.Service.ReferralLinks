using System.Threading.Tasks;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using AzureStorage;
using System;
using AutoMapper;
using Lykke.Service.ReferralLinks.AzureRepositories.DTOs;
using System.Collections.Generic;
using System.Linq;
using Lykke.SettingsReader;
using Lykke.Service.ReferralLinks.Core.Settings;
using Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings;

namespace Lykke.Service.ReferralLinks.AzureRepositories.ReferralLink
{
    public class ReferralLinkRepository : IReferralLinkRepository
    {
        private readonly IReloadingManager<ReferralLinksSettings> _settings;
        private readonly INoSQLTableStorage<ReferralLinkEntity> _referralLinkTable;

        public ReferralLinkRepository(
            INoSQLTableStorage<ReferralLinkEntity> referralLinkTable,
            IReloadingManager<ReferralLinksSettings> settings)
        {
            _referralLinkTable = referralLinkTable;
            _settings = settings;
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

            await _referralLinkTable.InsertOrReplaceAsync(entity);

            return entity.State;
        }

        public async Task<IReferralLink> Create(IReferralLink referralLink)
        {
            var entity = Mapper.Map<ReferralLinkEntity>(referralLink);

            entity.PartitionKey = GetPartitionKey();
            entity.RowKey = GetRowKey(referralLink.Id);

            if (entity.ExpirationDate == DateTime.MinValue || entity.ExpirationDate == DateTime.MaxValue)
                entity.ExpirationDate = DateTime.UtcNow.AddDays(_settings.CurrentValue.ExpirationDaysLimit);

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
            var numberOfNewUsersBroughtIn = referralLinks.Count(x => x.IsNewUser.HasValue && x.IsNewUser.Value);

            //TODO: Calculate amount of coins distributed
            var amountOfCoinsDistributed = 0;

            return new ReferralLinksStatisticsDto
            {
                AmountOfCoinsDistributed = amountOfCoinsDistributed,
                NumberOfInvitationAccepted = numberOfInvitationAccepted,
                NumberOfInvitationsSent = numberOfInvitationSent,
                NumberOfNewUsersBroughtIn = numberOfNewUsersBroughtIn
            };
        }

        public async Task<bool> IsReferralLinksNumberLimitReached(string claimingClientId)
        {
            var numberOfInitiatedAndCreatedReflinks = (await _referralLinkTable.GetDataAsync(
                GetPartitionKey(),
                x => x.State == ReferralLinkState.Initiated.ToString() || x.State == ReferralLinkState.Claimed.ToString()
            )).Count();

            return numberOfInitiatedAndCreatedReflinks > 100;
        }

        public async Task ReturnCoinsToSender()
        {
            var entities = await _referralLinkTable.GetDataAsync(
                GetPartitionKey(), 
                x => x.ExpirationDate < DateTime.UtcNow && x.State != ReferralLinkState.CoinsReturnedToSender.ToString()
            );

            foreach (var entity in entities)
            {
                await ClaimGiftCoins(entity.Id, false, entity.SenderClientId);
                await UpdateState(entity.Id, ReferralLinkState.CoinsReturnedToSender.ToString());
            }
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
