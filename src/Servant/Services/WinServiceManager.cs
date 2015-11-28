using Servant.Exceptions;
using Servant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;

namespace Servant.Services
{
    public class WinServiceManager
    {
        private static readonly TimeSpan ServiceStartTimeout = TimeSpan.FromSeconds(20);

        private static readonly TimeSpan ServiceStopTimeout = TimeSpan.FromSeconds(20);

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

        public void StartService(string serviceName)
        {
            var service = GetService(serviceName);
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, ServiceStartTimeout);
        }

        public void StopService(string serviceName)
        {
            var service = GetService(serviceName);
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, ServiceStopTimeout);
        }

        public void RestartService(string serviceName)
        {
            var service = GetService(serviceName);
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, ServiceStopTimeout);
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, ServiceStartTimeout);
        }

        private static ServiceController GetService(string serviceName)
        {
            var service = ServiceController.GetServices()
                .FirstOrDefault(x => x.ServiceName.Equals(serviceName, StringComparison.InvariantCultureIgnoreCase));

            if (service == null)
                throw new ServantException("The service is not found.");
            return service;
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
