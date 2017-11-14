using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Blue.Service.ReferralLinks.Core.Kyc
{
    public interface ISkipKycRepository
    {
        Task<bool> CanSkipKyc(string clientId);
        Task SkipKyc(string clientId, bool skip);
    }
}
