using Common;
using Common.Log;
using Lykke.blue.Service.ReferralLinks.Core.Domain.Offchain;
using Lykke.blue.Service.ReferralLinks.Extensions;
using Lykke.blue.Service.ReferralLinks.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Controllers
{
    public class RefLinksBaseController : Controller
    {
        private readonly ILog _log;

        public RefLinksBaseController(ILog log)
        {
            _log = log;
        }

        protected async Task<ObjectResult> LogAndReturnNotFound<T>(T request, ControllerContext controllerCtx, string info)
        {
            await LogInfo(request, controllerCtx,info);
            return NotFound(ErrorResponseModel.Create(info));
        }

        protected async Task<ObjectResult> LogOffchainExceptionAndReturn<T>(T request, ControllerContext controllerCtx, OffchainException ex)
        {
            await LogError(request, controllerCtx, new Exception($"OffchainException: {ex.ToJson()}"));
            return NotFound(ErrorResponseModel.Create((new { ex.OffchainExceptionMessage, ex.OffchainExceptionCode, ex.Message }).ToJson()));
        }

        protected async Task<ObjectResult> LogAndReturnBadRequest<T>(T request, ControllerContext controllerCtx, string info)
        {
            await LogInfo(request, controllerCtx, info);
            return BadRequest(ErrorResponseModel.Create(info));
        }

        protected async Task<ObjectResult> LogAndReturnInternalServerError<T>(T callParams, ControllerContext controllerCtx, string error)
        {
            await LogError(callParams, controllerCtx, new Exception(error));
            return StatusCode((int)HttpStatusCode.InternalServerError, ErrorResponseModel.Create(error));
        }
 
        protected async Task LogInfo<T>(T callParams, ControllerContext controllerCtx, string info)
        {
            await _log.WriteInfoAsync(controllerCtx.GetControllerAndAction(), (new { callParams }).ToJson(), info);
        }

        protected async Task LogWarn<T>(T callParams, ControllerContext controllerCtx, string info)
        {
            await _log.WriteWarningAsync(controllerCtx.GetControllerAndAction(), (new { callParams }).ToJson(), info);
        }

        protected async Task LogError<T>(T callParams, ControllerContext controllerCtx, Exception ex)
        {
            await _log.WriteErrorAsync(controllerCtx.GetControllerAndAction(), (new { callParams }).ToJson(), ex);
        }
    }
}
