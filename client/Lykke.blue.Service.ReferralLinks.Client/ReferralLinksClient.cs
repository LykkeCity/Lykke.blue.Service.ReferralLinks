using Common.Log;
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient;
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;
using System;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Client
{
    public class ReferralLinksClient : IReferralLinksClient, IDisposable
    {
        private readonly ILog _log;
        private LykkeReferralLinksService _service;

        public ReferralLinksClient(string serviceUrl, ILog log)
        {
            _log = log;
            _service = new LykkeReferralLinksService(new Uri(serviceUrl));
        }

        public Task<object> ClaimGiftCoins(ClaimReferralLinkRequest request)
        {
            throw new NotImplementedException("Reserved for version 2");
            //try
            //{
            //    return await _service.ClaimGiftCoinsAsync(request);
            //}
            //catch (Exception ex)
            //{
            //    await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetReferralLinksStatisticsBySenderId), ex);
            //    throw;
            //}
        }

        public Task<object> ClaimInvitationLink(ClaimReferralLinkRequest request)
        {
            throw new NotImplementedException("Reserved for version 2");
            //try
            //{
            //    return await _service.ClaimInvitationLinkAsync(request);
            //}
            //catch (Exception ex)
            //{
            //    await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(ClaimInvitationLink), ex);
            //    throw;
            //}
        }

        public void Dispose()
        {
            if (_service == null)
                return;
            _service.Dispose();
            _service = null;
        }

        public Task<object> FinalizeRefLinkTransfer(OffchainFinalizeModel request)
        {
            throw new NotImplementedException("Reserved for version 2");
            //try
            //{
            //    return await _service.FinalizeRefLinkTransferAsync(request);
            //}
            //catch (Exception ex)
            //{
            //    await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetChannelKey), ex);
            //    throw;
            //}
        }

        public Task<OffchainEncryptedKeyRespModel> GetChannelKey(OffchainGetChannelKeyRequest request)
        {
            throw new NotImplementedException("Reserved for version 2");
            //try
            //{
            //    return await _service.GetChannelKeyAsync(request);
            //}
            //catch (Exception ex)
            //{
            //    await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetChannelKey), ex);
            //    throw;
            //}
        }

        public async Task<object> GetReferralLink(string id)
        {
            try
            {
                return await _service.GetReferralLinkByIdAsync(id);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetReferralLink), ex);
                throw;
            }
        }       

        public async Task<object> GetReferralLinksStatisticsBySenderId(RefLinkStatisticsRequest request)
        {
            try
            {
                return await _service.GetReferralLinksStatisticsBySenderIdAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetReferralLinksStatisticsBySenderId), ex);
                throw;
            }
        }

        public Task<object> ProcessChannel(OffchainChannelProcessModel request)
        {
            throw new NotImplementedException("Reserved for version 2");
            //try
            //{
            //    return await _service.ProcessChannelAsync(request);
            //}
            //catch (Exception ex)
            //{
            //    await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(ProcessChannel), ex);
            //    throw;
            //}
        }

        public Task<object> RequestGiftCoinsReferralLink(GiftCoinsReferralLinkRequest request)
        {
            throw new NotImplementedException("Reserved for version 2");
            //try
            //{
            //    return await _service.RequestGiftCoinsReferralLinkAsync(request);
            //}
            //catch (Exception ex)
            //{
            //    await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(RequestGiftCoinsReferralLink), ex);
            //    throw;
            //}
        }

        public async Task<object> RequestInvitationReferralLink(InvitationReferralLinkRequest request)
        {
            try
            {
                return await _service.RequestInvitationReferralLinkAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(RequestInvitationReferralLink), ex);
                throw;
            }
        }

       

        public Task<object> TransferToLykkeWallet(TransferToLykkeWallet request)
        {
            throw new NotImplementedException("Reserved for version 2");
            //try
            //{
            //    return await _service.TransferToLykkeWalletMethodAsync(request);
            //}
            //catch (Exception ex)
            //{
            //    await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(TransferToLykkeWallet), ex);
            //    throw;
            //}
        }

    }
}
