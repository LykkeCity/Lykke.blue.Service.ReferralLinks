using Common;
using Common.Log;
using Lykke.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.Service.ReferralLinks.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ReferralLinks.Controllers
{
    public class RefLinksBaseController : Controller
    {
        private readonly ILog _log;

        public RefLinksBaseController(ILog log)
        {
            _log = log;
        }

        protected async Task<ObjectResult> LogAndReturnInternalServerError<T>(T request, ControllerContext controllerCtx, Exception ex)
        {
            await LogError(request, controllerCtx, ex);
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }

        protected async Task<ObjectResult> LogOffchainExceptionAndReturn<T>(T request, ControllerContext controllerCtx, OffchainException ex)
        {
            await LogError(request, controllerCtx, ex);
            return StatusCode((int)HttpStatusCode.InternalServerError, new { ex.OffchainExceptionMessage, ex.OffchainExceptionCode, ex.Message } );
        }

        protected async Task<ObjectResult> LogAndReturnBadRequest<T>(T request, ControllerContext controllerCtx, string info)
        {
            await LogWarn(request, controllerCtx, info);
            return BadRequest(info);
        }

        protected async Task LogInfo<T>(T callParams, ControllerContext controllerCtx, string info)
        {
            await _log.WriteInfoAsync(controllerCtx.GetExecutongControllerAndAction(), (new { callParams }).ToJson(), info, DateTime.Now);
        }

        protected async Task LogWarn<T>(T callParams, ControllerContext controllerCtx, string info)
        {
            await _log.WriteWarningAsync(controllerCtx.GetExecutongControllerAndAction(), (new { callParams }).ToJson(), info, DateTime.Now);
        }

        protected async Task LogError<T>(T callParams, ControllerContext controllerCtx, Exception ex)
        {
            await _log.WriteErrorAsync(controllerCtx.GetExecutongControllerAndAction(), (new { callParams }).ToJson(), ex, DateTime.Now);
        }
    }
}
