using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;
using Servant.Common.Entities;
using Servant.Exceptions;
using Servant.RequestParams;
using Servant.Services.WinService;
using Servant.Validation;

namespace Servant.Controllers
{
    [ApiExceptionFilter]
    [ValidationActionFilter]
    [RoutePrefix("api/winservices")]
    public class WinServicesController : ApiController
    {
        private readonly WinServiceManager _serviceManager;

        public WinServicesController()
        {
            _serviceManager = new WinServiceManager();
        }

        [HttpGet]
        [Route("")]
        public IEnumerable<WinServiceSimpleInfoModel> GetServices([FromUri]string q = "")
        {
            return _serviceManager.GetServices(q);
        }

        [HttpGet]
        [Route("{serviceName}")]
        public IHttpActionResult GetServiceInfo([Required]string serviceName)
        {
            var service = _serviceManager.GetServiceInfo(serviceName);
            if (service == null)
                return NotFound();

            return Ok(service);
        }

        [HttpPost]
        [Route("{serviceName}")]
        public IHttpActionResult PostCommand([FromUri][Required]string serviceName, [FromBody]WinServiceCommandRequest command)
        {
            if (command == null)
                return BadRequest("A value for action is not provided.");

            switch (command.Action.ToUpper())
            {
                case "START":
                    _serviceManager.StartService(serviceName);
                    break;
                case "STOP":
                    _serviceManager.StopService(serviceName);
                    break;
                case "RESTART":
                    _serviceManager.RestartService(serviceName);
                    break;
                case "SET-STARTTYPE":
                    SetStartType(serviceName, command.Value);
                    break;
                default:
                    return BadRequest($"Unknown action command - '{command.Action}'.");
            }
            return Ok();
        }

        private void SetStartType(string serviceName, string startType)
        {
            ServiceStartType startTypeEnum;
            if (string.IsNullOrEmpty(startType) || !Enum.TryParse(startType, true, out startTypeEnum))
                throw new ServantApiValidationException("A value for startType is invalid.");

            _serviceManager.SetStartType(serviceName, startType);
        }
    }
}
