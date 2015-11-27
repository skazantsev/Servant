using Servant.Models;
using Servant.Queries;
using Servant.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace Servant.Controllers
{
    public class WinServicesController : ApiController
    {
        public IEnumerable<WinService.SimpleInfo> GetServices([FromUri]WinServiceByNameQuery query)
        {
            var manager = new WinServiceManager();
            return manager.GetServices(query.Name);
        }

        public IHttpActionResult GetServiceInfo(string serviceName)
        {
            var manager = new WinServiceManager();
            var service = manager.GetServiceInfo(serviceName);
            if (service == null)
                return NotFound();

            return Ok(service);
        }
    }
}
