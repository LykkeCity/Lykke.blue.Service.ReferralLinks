﻿using AutoMapper;
using Common.Log;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ReferralLinks.Core.Domain.ReferralLink;
using Lykke.Service.ReferralLinks.Core.Services;
using Lykke.Service.ReferralLinks.Requests;
using Lykke.Service.ReferralLinks.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Controllers
{
    [Route("api/referralLinks")]
    public class ReferralLinksController : Controller
    {
        private readonly ILog _log;
        private readonly IReferralLinksService _referralLinksService;
        private readonly IClientAccountClient _clientAccountClient;

        public ReferralLinksController(
            ILog log,
            IReferralLinksService referralLinksService,
            IClientAccountClient clientAccountClient)
        {
            _log = log ?? throw new ArgumentException(nameof(log));
            _referralLinksService = referralLinksService ?? throw new ArgumentException(nameof(referralLinksService));
            _clientAccountClient = clientAccountClient ?? throw new ArgumentException(nameof(clientAccountClient));
        }

        /// <summary>
        /// Create referral link.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("CreateReferralLink")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(CreateReferralLinkResponse), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> Create([FromBody] CreateReferralLinkRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            //TODO: Add asset validation via assets client service here

            if (String.IsNullOrEmpty(request.SenderClientId) || await _clientAccountClient.GetClientById(request.SenderClientId) == null)
            {
                return BadRequest();
            }

            if (String.IsNullOrEmpty(request.RecipientClientIdOrEmail))
            {
                return BadRequest();
            }

            //TODO: Add EMAIL validation for RecipientClientIdOrEmail here
            if (await _clientAccountClient.GetClientById(request.RecipientClientIdOrEmail) == null)
            {
                return BadRequest();
            }

            var referralLink = Mapper.Map<CreateReferralLinkResponse>(await _referralLinksService.Create(request));

            return Created(uri: $"api/referralLinks/{referralLink.Id}", value: referralLink);
        }


        /// <summary>
        /// Get referral link.
        /// </summary>
        /// <param name="id">Id of a referral link we wanna get.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("GetReferralLink")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(GetReferralLinkResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var referralLink = await _referralLinksService.Get(id);

            if (referralLink == null)
            {
                return BadRequest();
            }

            var result = Mapper.Map<GetReferralLinkResponse>(referralLink);

            return Ok(result);
        }

        /// <summary>
        /// Get referral links by sender client id or state.
        /// </summary>
        /// <param name="senderClientId">Sender client id for which we wanna find referral links.</param>
        /// <param name="state">State by which we wanna to find referral links.</param>
        /// <returns></returns>
        /// REMARK: Swagger specification DOES NOT support optional parameters in path.
        [HttpGet]
        [SwaggerOperation("GetReferralLinksBySenderIdAndOrStatus")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<GetReferralLinkResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReferralLinksBySenderIdAndOrStatus([FromQuery] string senderClientId, [FromQuery] string state)
        {
            if (String.IsNullOrEmpty(senderClientId) && String.IsNullOrEmpty(state))
            {
                return BadRequest();
            }

            if (String.IsNullOrEmpty(senderClientId) && !String.IsNullOrEmpty(state) && !Enum.IsDefined(typeof(ReferralLinkState), state))
            {
                return BadRequest();
            }

            if (String.IsNullOrEmpty(state) && !String.IsNullOrEmpty(senderClientId) && await _clientAccountClient.GetClientById(senderClientId) == null)
            {
                return BadRequest();
            }

            if (!String.IsNullOrEmpty(senderClientId) && !String.IsNullOrEmpty(state)
                && (!Enum.IsDefined(typeof(ReferralLinkState), state) || await _clientAccountClient.GetClientById(senderClientId) == null))
            {
                return BadRequest();
            }

            var referralLinks = await _referralLinksService.GetReferralLinksBySenderClientIdAndOrStatus(senderClientId, state);

            var result = Mapper.Map<IEnumerable<GetReferralLinkResponse>>(referralLinks);

            return Ok(result);
        }

        /// <summary>
        /// Change referral link state.
        /// </summary>
        /// <param name="id">Id of a referral link we wanna change state for.</param>
        /// <param name="state">New referral link state.</param>
        /// <returns></returns>
        [HttpPut("updateState/{id}/{state}")]
        [SwaggerOperation("UpdateReferralLinkState")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdateState(string id, string state)
        {
            if (String.IsNullOrEmpty(id)
                || String.IsNullOrEmpty(state)
                || !Enum.IsDefined(typeof(ReferralLinkState), state))
            {
                return BadRequest();
            }

            var referralLink = await _referralLinksService.Get(id);

            if (referralLink == null)
            {
                return BadRequest();
            }

            await _referralLinksService.UpdateState(id, state);

            return NoContent();
        }

        /// <summary>
        /// Get referral links statistics by sender client id.
        /// </summary>
        /// <param name="senderClientId">Sender client id by which we wanna get statistics.</param>
        /// <returns></returns>
        [HttpGet("getReferralLinksStatistics/{senderClientId}")]
        [SwaggerOperation("GetReferralLinksStatisticsBySenderId")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(GetReferralLinksStatisticsBySenderIdResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReferralLinksStatisticsBySenderId(string senderClientId)
        {
            if (String.IsNullOrEmpty(senderClientId) || await _clientAccountClient.GetClientById(senderClientId) == null)
            {
                return BadRequest();
            }

            var referraLinksStatistics = await _referralLinksService.GetReferralLinksStatisticsBySenderId(senderClientId);

            return Ok(referraLinksStatistics);
        }

        /// <summary>
        /// Set referral link Url.
        /// </summary>
        /// <param name="id">Id of a referral link we wanna update url for.</param>
        /// <param name="url">Url that we wanna set.</param>
        /// <returns></returns>
        [HttpPut("setUrl/{id}/{url}")]
        [SwaggerOperation("SetReferralLinkUrl")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> SetUrl(string id, string url)
        {
            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(url))
            {
                return BadRequest();
            }

            var referralLink = await _referralLinksService.Get(id);

            if (referralLink == null)
            {
                return BadRequest();
            }

            await _referralLinksService.SetUrl(id, url);

            return NoContent();
        }
    }
}
