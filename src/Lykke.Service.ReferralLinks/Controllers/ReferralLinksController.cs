using AutoMapper;
using Common.Log;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ReferralLinks.Core.Services;
using Lykke.Service.ReferralLinks.Requests;
using Lykke.Service.ReferralLinks.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using System;
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
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
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
                return NotFound();
            }

            if (String.IsNullOrEmpty(request.RecipientClientIdOrEmail))
            {
                return NotFound();
            }

            //TODO: Add EMAIL validation for RecipientClientIdOrEmail here
            if (await _clientAccountClient.GetClientById(request.RecipientClientIdOrEmail) == null)
            {
                return NotFound();
            }

            var referralLink = Mapper.Map<CreateReferralLinkResponse>(await _referralLinksService.Create(request));

            return Created(uri: $"api/pledges/{referralLink.Id}", value: referralLink);
        }


        /// <summary>
        /// Get referral link.
        /// </summary>
        /// <param name="id">Id of a referral link we wanna get.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("GetReferralLink")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
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
                return NotFound();
            }

            var result = Mapper.Map<GetReferralLinkResponse>(referralLink);

            return Ok(result);
        }
    }
}
