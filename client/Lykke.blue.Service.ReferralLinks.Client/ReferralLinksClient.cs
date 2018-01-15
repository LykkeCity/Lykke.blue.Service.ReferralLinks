using Common.Log;
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient;
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;
using Lykke.blue.Service.ReferralLinks.Client.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
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

        public async Task<Microsoft.AspNetCore.Mvc.ObjectResult> ClaimInvitationLink(string refLinkId, ClaimReferralLinkRequest request)
        {
            try
            {
                var httpResponse = await _service.ClaimInvitationLinkWithHttpMessagesAsync(refLinkId,request);

                if (httpResponse.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new BadRequestObjectResult(await httpResponse.Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }

                var result = httpResponse.Body;

                if (result is ClaimRefLinkResponse) return new OkObjectResult(ClaimReferralLinkDto.Create(result as ClaimRefLinkResponse));

                throw new Exception();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(ClaimInvitationLink), ex);
                throw;
            }



        }

        public async Task<ReferralLinkDto> GetReferralLink(string id)
        {
            try
            {
                var res = await _service.GetReferralLinkByIdAsync(id);
                if (res is GetReferralLinkResponse) return ReferralLinkDto.Create(res as GetReferralLinkResponse);
                return null;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetReferralLink), ex);
                throw;
            }
        }

        public async Task<ReferralLinkDto> GetReferralLinkByUrl(string url)
        {
            try
            {
                var res = await _service.GetReferralLinkByUrlAsync(url);
                if (res is GetReferralLinkResponse) return ReferralLinkDto.Create(res as GetReferralLinkResponse);
                return null;
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

        public async Task<Microsoft.AspNetCore.Mvc.ObjectResult> RequestInvitationReferralLink(InvitationReferralLinkRequest request)
        {
            try
            {
                var httpResponse = await _service.RequestInvitationReferralLinkWithHttpMessagesAsync(request);

                if (httpResponse.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new BadRequestObjectResult(await httpResponse.Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }

                var result = httpResponse.Body;

                if (result is RequestRefLinkResponse) return new OkObjectResult(RequestReferralLinkDto.Create(result as RequestRefLinkResponse));

                throw new Exception();
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
