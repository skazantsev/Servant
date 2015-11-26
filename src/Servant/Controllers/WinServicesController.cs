using Servant.Queries;
using Servant.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace Servant.Controllers
{
    public class WinServicesController : ApiController
    {
        public IEnumerable<object> Get([FromUri]GetServiceInfoQuery query)
        {
            var manager = new WinServiceManager();
            return manager.GetServiceInfo(query.Name);
        }
    }
}
