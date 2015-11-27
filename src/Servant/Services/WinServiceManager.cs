using Servant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Servant.Services
{
    public class WinServiceManager
    {
        public List<WinService.SimpleInfo> GetServices(string serviceName)
        {
            serviceName = (serviceName ?? "").Replace("'", @"\'");
            var query = string.IsNullOrEmpty(serviceName)
                ? "SELECT * FROM Win32_Service"
                : $"SELECT * FROM Win32_Service WHERE Name LIKE '%{serviceName}%'";

            var services = GetServicesImpl(new SelectQuery(query));
            return services.Select(x => new WinService.SimpleInfo { ServiceName = x["Name"]?.ToString() }).ToList();
        }

        public WinService.FullInfo GetServiceInfo(string serviceName)
        {
            serviceName = (serviceName ?? "").Replace("'", @"\'");
            var query = $"SELECT * FROM Win32_Service WHERE Name='{serviceName}'";

            var service = GetServicesImpl(new SelectQuery(query)).FirstOrDefault();
            if (service == null)
                return null;

            return new WinService.FullInfo
            {
                ServiceName = service["Name"]?.ToString(),
                DisplayName = service["DisplayName"]?.ToString(),
                Description = service["Description"]?.ToString(),
                State = service["State"]?.ToString(),
                StartMode = service["StartMode"]?.ToString(),
                Account = service["StartName"]?.ToString(),
                PathName = service["PathName"]?.ToString()
            };
        }

        private static IEnumerable<ManagementBaseObject> GetServicesImpl(SelectQuery query)
        {
            var scope = new ManagementScope(@"root\CIMv2", new ConnectionOptions { Impersonation = ImpersonationLevel.Impersonate });
            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                return searcher.Get().Cast<ManagementBaseObject>();
            }
        }
    }
}
