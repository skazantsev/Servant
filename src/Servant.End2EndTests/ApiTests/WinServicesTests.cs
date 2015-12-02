using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Servant.Common.Entities;
using Servant.End2EndTests.Core;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading.Tasks;
using Xunit;

namespace Servant.End2EndTests.ApiTests
{
    public class WinServicesTests
    {
        private readonly RestApiTestClient _restApiClient;

        public WinServicesTests()
        {
            _restApiClient = new RestApiTestClient(new Uri("http://localhost:8025"));
        }

        [Fact]
        public async void When_GettingWinServices_Should_ReturnServiceNames()
        {
            var result = await _restApiClient.Get("/api/winservices");
            var services = JsonConvert.DeserializeObject<List<WinServiceSimpleInfo>>(result.Content);

            Assert.Equal(200, (int)result.Response.StatusCode);
            Assert.True(services.Count > 0);
            Assert.Contains(services, x => x.ServiceName == "eventlog");
        }

        [Fact]
        public async void When_GettingWinServiceInfo_Should_ReturnFullInfo()
        {
            var result = await _restApiClient.Get("/api/winservices/eventlog");
            var service = JsonConvert.DeserializeObject<WinServiceFullInfo>(result.Content);

            Assert.Equal(200, (int)result.Response.StatusCode);
            Assert.Equal("eventlog", service.ServiceName);
            Assert.Equal("Windows Event Log", service.DisplayName);
            Assert.NotNull(service.Account);
            Assert.NotNull(service.Description);
            Assert.NotNull(service.PathName);
            Assert.NotNull(service.StartMode);
            Assert.NotNull(service.State);
        }

        [Fact]
        public async void When_GettingServiceAndItDoesNotExist_Should_Return404()
        {
            var result = await _restApiClient.Get("/api/winservices/not-existing-service-name");
            Assert.Equal(404, (int)result.Response.StatusCode);
        }

        [Fact]
        public async void When_PostingActionAndServiceDoesNotExist_Should_Return500AndExceptionMessage()
        {
            var values = new KeyValueList<string, string>{ { "action", "start" } };
            var result = await _restApiClient.Post("/api/winservices/not-existing-service-name", values);
            var jcontent = JObject.Parse(result.Content);

            Assert.Equal(500, (int)result.Response.StatusCode);
            Assert.Equal(jcontent["ExceptionType"], "Servant.Exceptions.ServantException");
            Assert.NotNull(jcontent["ExceptionMessage"]);
        }

        [Fact]
        public async void When_PostingWithoutAction_Should_Return400AndValidationMessage()
        {
            var result = await _restApiClient.Post("/api/winservices/WinRM", new KeyValuePair<string, string>[] { });
            var jcontent = JObject.Parse(result.Content);

            Assert.Equal(400, (int)result.Response.StatusCode);
            Assert.Equal(jcontent["Message"], "A value for action is not provided.");
        }

        [Fact]
        public async void When_PostingUnknownAction_Should_Return400AndValidationMessage()
        {
            var values = new KeyValueList<string, string> { { "action", "unknown" } };
            var result = await _restApiClient.Post("/api/winservices/WinRM", values);
            var jcontent = JObject.Parse(result.Content);

            Assert.Equal(400, (int)result.Response.StatusCode);
            Assert.Equal(jcontent["Message"], "Unknown action command - 'unknown'.");
        }

        [Fact]
        public async void When_PostingStartActionAndServiceIsStopped_Should_StartService()
        {
            var serviceName = "WinRM";
            var values = new KeyValueList<string, string> { { "action", "start" } };

            EnsureServiceIsStopped(serviceName);
            await EnsureServiceIsNotDisabled(serviceName);

            var result = await _restApiClient.Post($"/api/winservices/{serviceName}", values);

            var serviceCtrl = new ServiceController(serviceName);
            Assert.Equal(200, (int)result.Response.StatusCode);
            Assert.Equal(serviceCtrl.Status, ServiceControllerStatus.Running);
        }

        [Fact]
        public async void When_PostingStartActionAndServiceIsRunning_Should_Return500AndExceptionMessage()
        {
            var serviceName = "WinRM";
            var values = new KeyValueList<string, string> { { "action", "start" } };

            EnsureServiceIsRunning(serviceName);
            await EnsureServiceIsNotDisabled(serviceName);

            var result = await _restApiClient.Post($"/api/winservices/{serviceName}", values);
            var jcontent = JObject.Parse(result.Content);

            Assert.Equal(500, (int)result.Response.StatusCode);
            Assert.Equal(jcontent["ExceptionType"], "System.InvalidOperationException");
            Assert.NotNull(jcontent["ExceptionMessage"]);
        }

        [Fact]
        public async void When_PostingRestartActionAndServiceIsRunning_Should_RestartService()
        {
            var serviceName = "WinRM";
            var values = new KeyValueList<string, string> { { "action", "restart" } };

            EnsureServiceIsRunning(serviceName);
            await EnsureServiceIsNotDisabled(serviceName);

            var result = await _restApiClient.Post($"/api/winservices/{serviceName}", values);

            var serviceCtrl = new ServiceController(serviceName);
            Assert.Equal(200, (int)result.Response.StatusCode);
            Assert.Equal(serviceCtrl.Status, ServiceControllerStatus.Running);
        }

        [Fact]
        public async void When_PostingRestartActionAndServiceIsStopped_Should_RestartService()
        {
            var serviceName = "WinRM";
            var values = new KeyValueList<string, string> { { "action", "restart" } };

            EnsureServiceIsStopped(serviceName);
            await EnsureServiceIsNotDisabled(serviceName);

            var result = await _restApiClient.Post($"/api/winservices/{serviceName}", values);
            var jcontent = JObject.Parse(result.Content);

            Assert.Equal(500, (int)result.Response.StatusCode);
            Assert.Equal(jcontent["ExceptionType"], "System.InvalidOperationException");
            Assert.NotNull(jcontent["ExceptionMessage"]);
        }

        [Fact]
        public async void When_PostingStopActionAndServiceIsRunning_Should_StopService()
        {
            var serviceName = "WinRM";
            var values = new KeyValueList<string, string> { { "action", "stop" } };

            EnsureServiceIsRunning(serviceName);
            await EnsureServiceIsNotDisabled(serviceName);

            var result = await _restApiClient.Post($"/api/winservices/{serviceName}", values);

            var serviceCtrl = new ServiceController(serviceName);
            Assert.Equal(200, (int)result.Response.StatusCode);
            Assert.Equal(serviceCtrl.Status, ServiceControllerStatus.Stopped);
        }

        [Fact]
        public async void When_PostingStopActionAndServiceIsStopped_Should_StopService()
        {
            var serviceName = "WinRM";
            var values = new KeyValueList<string, string> { { "action", "stop" } };

            EnsureServiceIsStopped(serviceName);
            await EnsureServiceIsNotDisabled(serviceName);

            var result = await _restApiClient.Post($"/api/winservices/{serviceName}", values);
            var jcontent = JObject.Parse(result.Content);

            Assert.Equal(500, (int)result.Response.StatusCode);
            Assert.Equal(jcontent["ExceptionType"], "System.InvalidOperationException");
            Assert.NotNull(jcontent["ExceptionMessage"]);
        }

        [Fact]
        public async void When_PostingSetStartTypeActionWithoutValue_Should_Return400AndValidationMessage()
        {
            var values = new KeyValueList<string, string> { { "action", "set-starttype" } };

            var result = await _restApiClient.Post("/api/winservices/WinRM", values);
            var jcontent = JObject.Parse(result.Content);

            Assert.Equal(400, (int)result.Response.StatusCode);
            Assert.Equal(jcontent["Message"], "A value for startType is invalid.");
        }

        [Fact]
        public async void When_PostingSetStartTypeActionAndValueIsUnknown_Should_Return400AndValidationMessage()
        {
            var values = new KeyValueList<string, string> {{"action", "set-starttype"}, {"value", "unknown"}};

            var result = await _restApiClient.Post("/api/winservices/WinRM", values);
            var jcontent = JObject.Parse(result.Content);

            Assert.Equal(400, (int)result.Response.StatusCode);
            Assert.Equal(jcontent["Message"], "A value for startType is invalid.");
        }

        [Fact]
        public async void When_PostingSetStartType_Should_SetServiceStartMode()
        {
            var serviceName = "WinRM";
            var values1 = new KeyValueList<string, string> {{"action", "set-starttype"}, {"value", "Automatic"}};
            var values2 = new KeyValueList<string, string> { { "action", "set-starttype" }, { "value", "Manual" } };
            var values3 = new KeyValueList<string, string> { { "action", "set-starttype" }, { "value", "Disabled" } };

            var result1 = await _restApiClient.Post($"/api/winservices/{serviceName}", values1);
            var serviceInfo1 = await GetService(serviceName);

            var result2 = await _restApiClient.Post($"/api/winservices/{serviceName}", values2);
            var serviceInfo2 = await GetService(serviceName);

            var result3 = await _restApiClient.Post($"/api/winservices/{serviceName}", values3);
            var serviceInfo3 = await GetService(serviceName);

            Assert.Equal(200, (int)result1.Response.StatusCode);
            Assert.Equal(200, (int)result2.Response.StatusCode);
            Assert.Equal(200, (int)result3.Response.StatusCode);
            Assert.Equal("Auto", serviceInfo1.StartMode);
            Assert.Equal("Manual", serviceInfo2.StartMode);
            Assert.Equal("Disabled", serviceInfo3.StartMode);
        }

        private static void EnsureServiceIsStopped(string serviceName)
        {
            var serviceCtrl = new ServiceController(serviceName);
            if (serviceCtrl.Status != ServiceControllerStatus.Stopped)
            {
                serviceCtrl.Stop();
                serviceCtrl.WaitForStatus(ServiceControllerStatus.Stopped);
            }
        }

        private static void EnsureServiceIsRunning(string serviceName)
        {
            var serviceCtrl = new ServiceController(serviceName);
            if (serviceCtrl.Status != ServiceControllerStatus.Running)
            {
                serviceCtrl.Start();
                serviceCtrl.WaitForStatus(ServiceControllerStatus.Running);
            }
        }

        private async Task EnsureServiceIsNotDisabled(string serviceName)
        {
            var service = await GetService(serviceName);
            if (service.StartMode == "Disabled")
            {
                var values = new KeyValueList<string, string> {{"action", "set-starttype"}, {"value", "Automatic"}};
                await _restApiClient.Post($"/api/winservices/{serviceName}", values);
            }
        }

        private async Task<WinServiceFullInfo> GetService(string serviceName)
        {
            var result = await _restApiClient.Get($"/api/winservices/{serviceName}");
            return JsonConvert.DeserializeObject<WinServiceFullInfo>(result.Content);
        }
    }
}
