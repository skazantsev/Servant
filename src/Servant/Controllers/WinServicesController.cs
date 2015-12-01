using Servant.Common.Entities;
using Servant.Exceptions;
using Servant.RequestParams;
using Servant.Services.WinService;
using Servant.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Web.Http;

namespace Servant.Controllers
{
    [ApiExceptionFilter]
    [ValidationActionFilter]
    public class WinServicesController : ApiController
    {
        private readonly WinServiceManager _serviceManager;

        public WinServicesController()
        {
            _serviceManager = new WinServiceManager();
        }

        [HttpGet]
        public IEnumerable<WinServiceSimpleInfo> GetServices([FromUri]WinServiceByNameRequest query)
        {
            return _serviceManager.GetServices(query.Name);
        }

        [HttpGet]
        public IHttpActionResult GetServiceInfo([Required]string serviceName)
        {
            var service = _serviceManager.GetServiceInfo(serviceName);
            if (service == null)
                return NotFound();

            return Ok(service);
        }

        [HttpPost]
        public IHttpActionResult PostCommand([FromUri][Required]string serviceName, [FromBody]WinServiceCommandRequest command)
        {
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
                    return BadRequest("Invalid command");
            }
            return Ok();
        }

        private void SetStartType(string serviceName, string startType)
        {
            ServiceStartType startTypeEnum;
            if (string.IsNullOrEmpty(startType) || !Enum.TryParse(startType, true, out startTypeEnum))
                throw new ServantApiException(HttpStatusCode.BadRequest, "A value for startType is invalid.");

            _serviceManager.SetStartType(serviceName, startType);
        }
    }
}
