using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using Servant.Common.Entities;
using Servant.Exceptions;

namespace Servant.Services.WinService
{
    public class WinServiceManager
    {
        private static readonly TimeSpan ServiceStartTimeout = TimeSpan.FromSeconds(20);

        private static readonly TimeSpan ServiceStopTimeout = TimeSpan.FromSeconds(20);

        public List<WinServiceSimpleInfo> GetServices(string serviceName)
        {
            var services = string.IsNullOrEmpty(serviceName)
                ? GetAllServices()
                : FindServices(serviceName);

            return services.Select(x => x.GetSimpleInfo()).ToList();
        }

        public WinServiceFullInfo GetServiceInfo(string serviceName)
        {
            var service = GetService(serviceName);
            return service?.GetFullInfo();
        }

        public void StartService(string serviceName)
        {
            var serviceCtrl = GetServiceController(serviceName);
            serviceCtrl.Start();
            serviceCtrl.WaitForStatus(ServiceControllerStatus.Running, ServiceStartTimeout);
        }

        public void StopService(string serviceName)
        {
            var serviceCtrl = GetServiceController(serviceName);
            serviceCtrl.Stop();
            serviceCtrl.WaitForStatus(ServiceControllerStatus.Stopped, ServiceStopTimeout);
        }

        public void RestartService(string serviceName)
        {
            var service = GetServiceController(serviceName);
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, ServiceStopTimeout);
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, ServiceStartTimeout);
        }

        public void SetStartType(string serviceName, string startType)
        {
            var service = GetService(serviceName);
            service.ChangeStartMode(startType);
        }

        private static IEnumerable<WinServiceItem> GetAllServices()
        {
            return QueryWmi(new SelectQuery("SELECT * FROM Win32_Service"));
        }

        private static IEnumerable<WinServiceItem> FindServices(string searchQuery)
        {
            return QueryWmi(new SelectQuery($"SELECT * FROM Win32_Service WHERE Name LIKE '%{EscapeQueryParam(searchQuery)}%'"));
        }

        private static WinServiceItem GetService(string serviceName)
        {
            var services = QueryWmi(new SelectQuery($"SELECT * FROM Win32_Service WHERE Name='{EscapeQueryParam(serviceName)}'"));
            return services.FirstOrDefault();
        }

        private static ServiceController GetServiceController(string serviceName)
        {
            var service = ServiceController.GetServices()
                .FirstOrDefault(x => x.ServiceName.Equals(serviceName, StringComparison.InvariantCultureIgnoreCase));

            if (service == null)
                throw new ServantException("The service is not found.");
            return service;
        }

        private static IEnumerable<WinServiceItem> QueryWmi(SelectQuery query)
        {
            var scope = new ManagementScope(@"root\CIMv2", new ConnectionOptions { Impersonation = ImpersonationLevel.Impersonate });
            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                return searcher.Get().Cast<ManagementObject>().Select(x => new WinServiceItem(x));
            }
        }

        private static string EscapeQueryParam(string queryParam)
        {
            return (queryParam ?? "").Replace("'", @"\'");
        }
    }
}
