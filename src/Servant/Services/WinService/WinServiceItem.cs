using System;
using System.Collections.Generic;
using System.Management;
using Servant.Common.Entities;
using Servant.Exceptions;

namespace Servant.Services.WinService
{
    public class WinServiceItem
    {
        private readonly ManagementObject _managementObject;

        public WinServiceItem(ManagementObject managementObject)
        {
            if (managementObject == null)
                throw new ArgumentNullException(nameof(managementObject));

            _managementObject = managementObject;
        }

        public WinServiceSimpleInfoModel GetSimpleInfo()
        {
            return new WinServiceSimpleInfoModel
            {
                ServiceName = _managementObject["Name"]?.ToString()
            };
        }

        public WinServiceFullInfoModel GetFullInfo()
        {
            return new WinServiceFullInfoModel
            {
                ServiceName = _managementObject["Name"]?.ToString(),
                DisplayName = _managementObject["DisplayName"]?.ToString(),
                Description = _managementObject["Description"]?.ToString(),
                State = _managementObject["State"]?.ToString(),
                StartMode = _managementObject["StartMode"]?.ToString(),
                Account = _managementObject["StartName"]?.ToString(),
                PathName = _managementObject["PathName"]?.ToString()
            };
        }

        public void ChangeStartMode(string startType)
        {
            var resultCode = (uint)_managementObject.InvokeMethod("ChangeStartMode", new object[] { startType });
            if (resultCode != 0)
                ThrowError(resultCode);
        }

        private static void ThrowError(uint errorCode)
        {
            string errorMessage;
            if (ErrorCodes.TryGetValue(errorCode, out errorMessage))
                throw new ServantException(errorMessage);

            throw new ServantException($"Unspecified error with code {errorCode}.");
        }

        private static readonly Dictionary<uint, string> ErrorCodes = new Dictionary<uint, string>
        {
            {1, "The request is not supported." },
            {2, "The user did not have the necessary access." },
            {3, "The service cannot be stopped because other services that are running are dependent on it." },
            {4, "The requested control code is not valid, or it is unacceptable to the service." },
            {5, "The requested control code cannot be sent to the service because of the state of the service." },
            {6, "The service has not been started." },
            {7, "The service did not respond to the start request in a timely fashion." },
            {8, "Unknown failure when starting the service." },
            {9, "The directory path to the service executable file was not found." },
            {10, "The service is already running." },
            {11, "The database to add a new service is locked." },
            {12, "A dependency this service relies on has been removed from the system." },
            {13, "The service failed to find the service needed from a dependent service." },
            {14, "The service has been disabled from the system." },
            {15, "The service does not have the correct authentication to run on the system." },
            {16, "This service is being removed from the system." },
            {17, "The service has no execution thread." },
            {18, "The service has circular dependencies when it starts." },
            {19, "A service is running under the same name." },
            {20, "The service name has invalid characters." },
            {21, "Invalid parameters have been passed to the service." },
            {22, "The account under which this service runs is either invalid or lacks the permissions to run the service." },
            {23, "The service exists in the database of services available from the system." },
            {24, "The service is currently paused in the system." }
        };
    }
}
