using Common.Log;
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient;
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;
using System;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global

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

        public void Dispose()
        {
            if (_service == null)
                return;
            _service.Dispose();
            _service = null;
        }

        public async Task<object> ClaimGiftCoins(string refLinkId, ClaimReferralLinkRequest request)
        {
            try
            {
                return await _service.ClaimGiftCoinsAsync(refLinkId, request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(ClaimGiftCoins), ex);
                throw;
            }
        }

        public async Task<object> ClaimInvitationLink(string refLinkId, ClaimReferralLinkRequest request)
        {
            try
            {
                return await _service.ClaimInvitationLinkAsync(refLinkId, request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(ClaimInvitationLink), ex);
                throw;
            }
        }

        public async Task<object> FinalizeGiftCoinLinkTransfer(OffchainFinalizeModel request)
        {
            try
            {
                return await _service.FinalizeRefLinkTransferAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(FinalizeGiftCoinLinkTransfer), ex);
                throw;
            }
        }

        public async Task<OffchainEncryptedKeyRespModel> GetChannelKey(string asset, string clientId)
        {
            try
            {
                return await _service.GetChannelKeyAsync(asset, clientId);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetChannelKey), ex);
                throw;
            }
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

        public async Task<object> GetReferralLinksStatisticsBySenderId(string senderClientId)
        {
            try
            {
                return await _service.GetReferralLinksStatisticsBySenderIdAsync(senderClientId);
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
                return await _service.TransferToLykkeHotWalletAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(TransferToLykkeWallet), ex);
                throw;
            }
        }

    }
}
