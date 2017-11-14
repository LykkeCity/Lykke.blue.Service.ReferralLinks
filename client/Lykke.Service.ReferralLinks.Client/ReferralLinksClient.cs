using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Blue.Service.ReferralLinks.Client.AutorestClient;
using Lykke.Blue.Service.ReferralLinks.Client.AutorestClient.Models;

namespace Lykke.Blue.Service.ReferralLinks.Client
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

        public async Task<string> ClaimGiftCoins(ClaimGiftCoinsRequest request)
        {
            try
            {
                return await _service.ClaimGiftCoinsAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(ClaimGiftCoins), ex);
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

        public async Task<GetReferralLinkResponse> Get(string id)
        {
            try
            {
                return await _service.GetReferralLinkAsync(id);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(Get), ex);
                throw;
            }
        }

        public async Task<IEnumerable<GetReferralLinkResponse>> GetReferralLinksBySenderIdAndOrStatus(string senderClientId, string state)
        {
            try
            {
                return await _service.GetReferralLinksBySenderIdAndOrStatusAsync(senderClientId, state);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetReferralLinksBySenderIdAndOrStatus), ex);
                throw;
            }
        }

        public async Task<GetReferralLinksStatisticsBySenderIdResponse> GetReferralLinksStatisticsBySenderId(string senderClientId)
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

        public async Task<string> RequestReferralLink(RequestReferralLinkRequest request)
        {
            try
            {
                return await _service.RequestReferralLinkAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(RequestReferralLink), ex);
                throw;
            }
        }

        public async Task SetUrl(string id, string url)
        {
            try
            {
                await _service.SetReferralLinkUrlAsync(id, url);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(SetUrl), ex);
                throw;
            }
        }

        public async Task UpdateState(string id, string state)
        {
            try
            {
                await _service.UpdateReferralLinkStateAsync(id, state);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(UpdateState), ex);
                throw;
            }
        }
    }
}
