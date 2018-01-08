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
        private ILykkeBlueServiceReferralLinks _service;

        public ReferralLinksClient(string serviceUrl, ILog log)
        {
            _log = log;
            _service = new LykkeBlueServiceReferralLinks(new Uri(serviceUrl));
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

        public async Task<object> GetReferralLinkByUrl(string url)
        {
            try
            {
                return await _service.GetReferralLinkByUrlAsync(url);
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


        public async Task<object> RequestGiftCoinsReferralLink(GiftCoinRequest request)
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

        public async Task<object> GroupGenerateGiftCoinLinks(GiftCoinRequestGroup request)
        {
            try
            {
                return await _service.GroupGenerateGiftCoinLinksAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(RequestInvitationReferralLink), ex);
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

        public async Task<object> GetGiftCoinReferralLinks(string senderClientId)
        {
            try
            {
                return await _service.GetGroupReferralLinkBySenderIdAsync(senderClientId);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(RequestInvitationReferralLink), ex);
                throw;
            }
        }
    }
}
