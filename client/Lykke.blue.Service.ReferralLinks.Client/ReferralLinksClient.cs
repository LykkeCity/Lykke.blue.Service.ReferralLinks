using Common.Log;
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient;
using Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models;
using Lykke.blue.Service.ReferralLinks.Client.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var srv = new LykkeBlueServiceReferralLinks(new Uri(serviceUrl));
            srv.SetRetryPolicy(null); //prevent autorest client from retrying Internal server error 500 responses
            _service = srv;

        }

        public void Dispose()
        {
            if (_service == null)
                return;
            _service.Dispose();
            _service = null;
        }

        public async Task<Microsoft.AspNetCore.Mvc.ObjectResult> ClaimGiftCoinsAsync(string refLinkId, ClaimReferralLinkRequest request)
        {
            try
            {
                var httpResponse = await _service.ClaimGiftCoinsWithHttpMessagesAsync(refLinkId, request);

                if (httpResponse.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new BadRequestObjectResult(await httpResponse.Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }

                var result = httpResponse.Body;

                if (httpResponse.Response.StatusCode == HttpStatusCode.OK && result is ClaimRefLinkResponse)
                {
                    return new OkObjectResult(ClaimReferralLinkDto.Create((ClaimRefLinkResponse) result));
                }

                throw new Exception(httpResponse.Body.ToString());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(ClaimGiftCoinsAsync), ex);
                throw;
            }
        }

        public async Task<Microsoft.AspNetCore.Mvc.ObjectResult> ClaimInvitationLinkAsync(string refLinkId, ClaimReferralLinkRequest request)
        {
            try
            {
                var httpResponse = await _service.ClaimInvitationLinkWithHttpMessagesAsync(refLinkId,request);

                if (httpResponse.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new BadRequestObjectResult(await httpResponse.Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }

                var result = httpResponse.Body;

                if (httpResponse.Response.StatusCode == HttpStatusCode.OK && result is ClaimRefLinkResponse)
                {
                    return new OkObjectResult(ClaimReferralLinkDto.Create((ClaimRefLinkResponse) result));
                }

                throw new Exception(httpResponse.Body.ToString());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(ClaimInvitationLinkAsync), ex);
                throw;
            }



        }

        public async Task<ReferralLinkDto> GetReferralLinkAsync(string id)
        {
            try
            {
                var res = await _service.GetReferralLinkByIdAsync(id);
                if (res is GetReferralLinkResponse) return ReferralLinkDto.Create(res as GetReferralLinkResponse);
                return null;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetReferralLinkAsync), ex);
                throw;
            }
        }

        public async Task<ReferralLinkDto> GetReferralLinkByUrlAsync(string url)
        {
            try
            {
                var res = await _service.GetReferralLinkByUrlAsync(url);
                if (res is GetReferralLinkResponse) return ReferralLinkDto.Create(res as GetReferralLinkResponse);
                return null;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetReferralLinkAsync), ex);
                throw;
            }
        }

        public async Task<ReferralLinksStatisticsDto> GetReferralLinksStatisticsBySenderIdAsync(string senderClientId)
        {
            try
            {
                var result = await _service.GetReferralLinksStatisticsBySenderIdAsync(senderClientId);
                if (result is GetReferralLinksStatisticsBySenderIdResponse) return ReferralLinksStatisticsDto.Create(result as GetReferralLinksStatisticsBySenderIdResponse);
                return null;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetReferralLinksStatisticsBySenderIdAsync), ex);
                throw;
            }
        }


        public async Task<Microsoft.AspNetCore.Mvc.ObjectResult> RequestGiftCoinsReferralLinkAsync(GiftCoinRequest request)
        {
            try
            {
                var httpResponse = await _service.RequestGiftCoinsReferralLinkWithHttpMessagesAsync(request);

                if (httpResponse.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new BadRequestObjectResult(await httpResponse.Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }

                var result = httpResponse.Body;

                if (httpResponse.Response.StatusCode == HttpStatusCode.Created && result is RequestRefLinkResponse)
                {
                    return new CreatedResult(httpResponse.Response.Headers.Location, RequestReferralLinkDto.Create((RequestRefLinkResponse) result));
                }

                throw new Exception(httpResponse.Body.ToString());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(RequestGiftCoinsReferralLinkAsync), ex);
                throw;
            }
        }

        public async Task<Microsoft.AspNetCore.Mvc.ObjectResult> GroupGenerateGiftCoinLinksAsync(GiftCoinRequestGroup request)
        {
            try
            {
                var httpResponse = await _service.GroupGenerateGiftCoinLinksWithHttpMessagesAsync(request);

                if (httpResponse.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new BadRequestObjectResult(await httpResponse.Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }

                if (httpResponse.Response.StatusCode == HttpStatusCode.Created)
                {
                    return new CreatedResult(httpResponse.Response.Headers.Location, "");
                }

                throw new Exception(httpResponse.Body.ToString());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GroupGenerateGiftCoinLinksAsync), ex);
                throw;
            }
        }

        public async Task<Microsoft.AspNetCore.Mvc.ObjectResult> RequestInvitationReferralLinkAsync(InvitationReferralLinkRequest request)
        {
            try
            {
                var httpResponse = await _service.RequestInvitationReferralLinkWithHttpMessagesAsync(request);

                if (httpResponse.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new BadRequestObjectResult(await httpResponse.Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }

                var result = httpResponse.Body;

                if (httpResponse.Response.StatusCode == HttpStatusCode.Created && result is RequestRefLinkResponse)
                {
                    return new CreatedResult(httpResponse.Response.Headers.Location, RequestReferralLinkDto.Create(result as RequestRefLinkResponse));
                }

                if (httpResponse.Response.StatusCode == HttpStatusCode.OK && result is RequestRefLinkResponse)
                {
                    return new OkObjectResult(RequestReferralLinkDto.Create(result as RequestRefLinkResponse));
                }

                throw new Exception();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(RequestInvitationReferralLinkAsync), ex);
                throw;
            }
        }

        public async Task<IEnumerable<ReferralLinkDto>> GetGiftCoinReferralLinksAsync(string senderClientId)
        {
            try
            {
                var res = await _service.GetGroupReferralLinkBySenderIdAsync(senderClientId);
                if(res is IEnumerable<GetReferralLinkResponse>) return new List<ReferralLinkDto>( (res as IEnumerable<GetReferralLinkResponse>).Select(ReferralLinkDto.Create)  );
                return null;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ReferralLinksClient), nameof(GetGiftCoinReferralLinksAsync), ex);
                throw;
            }
        }
    }
}
