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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace Lykke.Service.ReferralLinks.AzureRepositories.ReferralLink
{
    public class ReferralLinkRepository : IReferralLinkRepository
    {
        private readonly INoSQLTableStorage<ReferralLinkEntity> _referralLinkTable;
        private readonly ReferralLinksSettings _settings;

        public ReferralLinkRepository(INoSQLTableStorage<ReferralLinkEntity> referralLinkTable, ReferralLinksSettings settings)
        {
            _referralLinkTable = referralLinkTable;
            _settings = settings;
        }

        public static string GetPartitionKey() => "ReferallLink";

        public static string GetRowKey(string id) => id;
        //{
        //    return String.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        //}

        //public async Task<string> ClaimGiftCoins(string referralLinkId, bool newUser, string claimingClientId)
        //{
        //    var entity = await _referralLinkTable.GetDataAsync(GetPartitionKey(), GetRowKey(referralLinkId));
        //    entity.IsNewUser = newUser;
        //    entity.ClaimingClientId = claimingClientId;            

        //    await _referralLinkTable.InsertOrReplaceAsync(entity);

        //    return entity.State;
        //}

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

        public async Task<IEnumerable<IReferralLink>> Get(string senderClientId, ReferralLinkState? state)
        {
            var entities = await _referralLinkTable.GetDataAsync(
                GetPartitionKey(),
                x => (String.IsNullOrEmpty(senderClientId) || x.SenderClientId == senderClientId) && (!state.HasValue || x.State == state.Value)
            );

            return Mapper.Map<IEnumerable<ReferralLinkDto>>(entities);
        }

        public async Task<IReferralLink> GetReferalLinkByUrl(string url)
        {
            var entities = await _referralLinkTable.GetDataAsync(
                GetPartitionKey(),
                x => x.Url == url);

            return Mapper.Map<ReferralLinkDto>(entities.FirstOrDefault());
        }

        public async Task<IReferralLinksStatistics> GetReferralLinksStatisticsBySenderId(string senderClientId)
        {
            var referralLinks = await _referralLinkTable.GetDataAsync(
                GetPartitionKey(),
                x => x.SenderClientId == senderClientId
            );

            var numberOfInvitationSent = referralLinks.Count(x => x.State == ReferralLinkState.SentToLykkeSharedWallet);
            var numberOfInvitationAccepted = referralLinks.Count(x => x.State == ReferralLinkState.Claimed);
            //var numberOfNewUsersBroughtIn = referralLinks.Count(x => x.IsNewUser.HasValue && x.IsNewUser.Value); //this should come from ReferralLinkClaimsRepository
            var amountOfCoinsDistributed = referralLinks
                .Where(x => x.State == ReferralLinkState.Claimed)
                .Sum(x => x.Amount);

            return new ReferralLinksStatisticsDto
            {
                AmountOfCoinsDistributed = amountOfCoinsDistributed,
                NumberOfInvitationAccepted = numberOfInvitationAccepted,
                NumberOfInvitationsSent = numberOfInvitationSent,
                //NumberOfNewUsersBroughtIn = numberOfNewUsersBroughtIn //this should come from ReferralLinkClaimsRepository
            };
        }

        public async Task<bool> IsReferralLinksNumberLimitReached(string senderClientId)
        {
            var numberOfCreatedReflinks = (await _referralLinkTable.GetDataAsync(
                GetPartitionKey(),
                x => x.SenderClientId == senderClientId 
                    && x.State == ReferralLinkState.Created
                    && x.Type == ReferralLinkType.Invitation
            )).Count();

            return numberOfCreatedReflinks >= _settings.ReferralLinksNumberLimit;
        }

        public async Task ReturnCoinsToSender()
        {
            var entities = await _referralLinkTable.GetDataAsync(
                GetPartitionKey(), 
                x => x.ExpirationDate < DateTime.UtcNow && x.State != ReferralLinkState.CoinsReturnedToSender
            );

            foreach (var entity in entities)
            {
                //await ClaimGiftCoins(entity.Id, false, entity.SenderClientId);
                await UpdateState(entity.Id, ReferralLinkState.CoinsReturnedToSender);
            }
        }

        public async Task SetUrl(string id, string url)
        {
            var entity = await _referralLinkTable.GetDataAsync(GetPartitionKey(), GetRowKey(id));
            entity.Url = url;

            await _referralLinkTable.InsertOrReplaceAsync(entity);
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

        public async Task UpdateState(string id, ReferralLinkState state)
        {
            var entity = await _referralLinkTable.GetDataAsync(GetPartitionKey(), GetRowKey(id));
            entity.State = state;

            await _referralLinkTable.InsertOrReplaceAsync(entity);
        }
    }
}
