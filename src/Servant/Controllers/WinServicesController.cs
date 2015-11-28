using Servant.Models;
using Servant.RequestParams;
using Servant.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace Servant.Controllers
{
    public class WinServicesController : ApiController
    {
        [HttpGet]
        public IEnumerable<WinService.SimpleInfo> GetServices([FromUri]WinServiceByNameRequest query)
        {
            var manager = new WinServiceManager();
            return manager.GetServices(query.Name);
        }

        [HttpGet]
        public IHttpActionResult GetServiceInfo(string serviceName)
        {
            var manager = new WinServiceManager();
            var service = manager.GetServiceInfo(serviceName);
            if (service == null)
                return NotFound();

            return Ok(service);
        }

        [HttpPost]
        public IHttpActionResult PostCommand([FromUri]string serviceName, [FromBody]WinServiceCommandRequest command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var manager = new WinServiceManager();
            switch (command.Action.ToUpper())
            {
                case "START":
                    manager.StartService(serviceName);
                    break;
                case "STOP":
                    manager.StopService(serviceName);
                    break;
                case "RESTART":
                    manager.RestartService(serviceName);
                    break;
                default:
                    return BadRequest("Invalid command");
            }
            return Ok();
        }
    }
}
