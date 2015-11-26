using Servant.Models;
using System.Collections.Generic;
using System.Management;

namespace Servant.Services
{
    public class WinServiceManager
    {
        public IEnumerable<object> GetServiceInfo(string name)
        {
            name = (name ?? "").Replace("'", @"\'");
            var scope = new ManagementScope(@"root\CIMv2", new ConnectionOptions {Impersonation = ImpersonationLevel.Impersonate});

            var wmiQuery = string.IsNullOrEmpty(name)
                ? "SELECT * FROM Win32_Service"
                : $"SELECT * FROM Win32_Service WHERE DisplayName LIKE '%{name}%'";
            using (var searcher = new ManagementObjectSearcher(scope, new SelectQuery(wmiQuery)))
            {
                var foundServices = searcher.Get();
                if (foundServices.Count > 1)
                {
                    foreach (var service in foundServices)
                    {
                        yield return new WinServiceSimpleInfo { DisplayName = service["DisplayName"]?.ToString() };
                    }
                }
                else
                {
                    foreach (var service in foundServices)
                    {
                        yield return new WinServiceFullInfo
                        {
                            DisplayName = service["DisplayName"]?.ToString(),
                            ServiceName = service["Name"]?.ToString(),
                            Description = service["Description"]?.ToString(),
                            State = service["State"]?.ToString(),
                            StartMode = service["StartMode"]?.ToString(),
                            Account = service["StartName"]?.ToString(),
                            PathName = service["PathName"]?.ToString()
                        };
                    }
                }
            }
        }
    }
}
