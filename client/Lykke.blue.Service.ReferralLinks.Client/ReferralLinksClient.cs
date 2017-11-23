using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient;
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;

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

        public async Task<object> ClaimGiftCoins(ClaimReferralLinkRequest request)
        {
            try
            {
                return await _service.ClaimGiftCoinsAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetReferralLinksStatisticsBySenderId), ex);
                throw;
            }
        }

        public async Task<object> ClaimInvitationLink(ClaimReferralLinkRequest request)
        {
            try
            {
                return await _service.ClaimInvitationLinkAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(ClaimInvitationLink), ex);
                throw;
            }
        }



        //public async Task<CreateReferralLinkResponse> Create(CreateReferralLinkRequest request)
        //{
        //    throw new NotImplementedException();
        //}

        public void Dispose()
        {
            if (_service == null)
                return;
            _service.Dispose();
            _service = null;
        }

        public async Task<object> FinalizeRefLinkTransfer(OffchainFinalizeModel request)
        {
            try
            {
                return await _service.FinalizeRefLinkTransferAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetChannelKey), ex);
                throw;
            }
        }

        public async Task<OffchainEncryptedKeyRespModel> GetChannelKey(OffchainGetChannelKeyRequest request)
        {
            try
            {
                return await _service.GetChannelKeyAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetChannelKey), ex);
                throw;
            }
        }

        public async Task<GetReferralLinkResponse> GetReferralLink(string id)
        {
            try
            {
                return await _service.GetReferralLinkAsync(id);
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

        public async Task<object> ProcessChannel(OffchainChannelProcessModel request)
        {
            try
            {
                return await _service.ProcessChannelAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(ProcessChannel), ex);
                throw;
            }
        }

        public async Task<object> RequestGiftCoinsReferralLink(GiftCoinsReferralLinkRequest request)
        {
            try
            {
                return await _service.RequestGiftCoinsReferralLinkAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(RequestGiftCoinsReferralLink), ex);
                throw;
            }
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

       

        public async Task<object> TransferToLykkeWallet(TransferToLykkeWallet request)
        {
            try
            {
                return await _service.TransferToLykkeWalletMethodAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(TransferToLykkeWallet), ex);
                throw;
            }
        }

    }
}
