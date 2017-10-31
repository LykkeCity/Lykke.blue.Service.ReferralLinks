using AzureStorage.Queue;
using Lykke.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.Service.ReferralLinks.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.AzureRepositories.Offchain
{
    public class OffchainFinalizeCommandProducer : IOffchainFinalizeCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public OffchainFinalizeCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;
        }

        public async Task ProduceFinalize(string transferId, string clientId, string hash)
        {
            await _queueExt.PutRawMessageAsync(new OfchainFinalizeTransactionCommand
            {
                ClientId = clientId,
                TransferId = transferId,
                TransactionHash = hash
            }.ToJson());
        }

        public class OfchainFinalizeTransactionCommand
        {
            public string ClientId { get; set; }
            public string TransferId { get; set; }
            public string TransactionHash { get; set; }
        }
    }
}
