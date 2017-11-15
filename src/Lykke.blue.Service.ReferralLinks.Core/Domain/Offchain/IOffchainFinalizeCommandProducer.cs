using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain
{
    public interface IOffchainFinalizeCommandProducer
    {
        Task ProduceFinalize(string transferId, string clientId, string hash);
    }
}
