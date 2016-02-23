using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;
using System.Web.Http.ModelBinding.Binders;
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
        public IEnumerable<WinServiceSimpleInfoModel> GetServices([FromUri(BinderType = typeof(TypeConverterModelBinder))]string q = "")
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
        [Route("{serviceName}/start")]
        public IHttpActionResult Start([FromUri][Required]string serviceName)
        {
            _serviceManager.StartService(serviceName);
            return Ok();
        }

        [HttpPost]
        [Route("{serviceName}/stop")]
        public IHttpActionResult Stop([FromUri][Required]string serviceName)
        {
            _serviceManager.StopService(serviceName);
            return Ok();
        }

        [HttpPost]
        [Route("{serviceName}/restart")]
        public IHttpActionResult Restart([FromUri][Required]string serviceName)
        {
            _serviceManager.RestartService(serviceName);
            return Ok();
        }

        [HttpPost]
        [ModelRequiredFilter]
        [Route("{serviceName}/setStartType")]
        public IHttpActionResult SetStartType([FromUri][Required]string serviceName, [FromBody]SetStartTypeRequest request)
        {
            _serviceManager.SetStartType(serviceName, Enum.GetName(typeof(ServiceStartType), request.StartType));
            return Ok();
        }
    }
}
