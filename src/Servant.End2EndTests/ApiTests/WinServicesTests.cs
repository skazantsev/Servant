using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceProcess;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Servant.Common.Entities;
using Servant.End2EndTests.Core;
using Servant.End2EndTests.Helpers;
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
            var services = JsonConvert.DeserializeObject<List<WinServiceSimpleInfoModel>>(result.Content);

            Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.True(services.Count > 0);
            Assert.Contains(services, x => x.ServiceName == "eventlog");
        }

        [Fact]
        public async void When_GettingWinServicesWithEmptySearchQueryParameter_Should_ReturnServiceNames()
        {
            var result = await _restApiClient.Get("/api/winservices?q=");
            var services = JsonConvert.DeserializeObject<List<WinServiceSimpleInfoModel>>(result.Content);

            Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.True(services.Count > 0);
            Assert.Contains(services, x => x.ServiceName == "eventlog");
        }

        [Fact]
        public async void When_GettingWinServicesWithSearchQueryParameter_Should_ReturnFilteredServiceNames()
        {
            var result = await _restApiClient.Get("/api/winservices?q=winrm");
            var services = JsonConvert.DeserializeObject<List<WinServiceSimpleInfoModel>>(result.Content);

            Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.True(services.Count > 0);
            Assert.Contains(services, x => x.ServiceName == "WinRM");
            Assert.DoesNotContain(services, x => x.ServiceName == "eventlog");
        }

        [Fact]
        public async void When_GettingWinServiceInfo_Should_ReturnFullInfo()
        {
            var result = await _restApiClient.Get("/api/winservices/eventlog");
            var service = JsonConvert.DeserializeObject<WinServiceFullInfoModel>(result.Content);

            Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
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
            Assert.Equal(HttpStatusCode.NotFound, result.Response.StatusCode);
        }

        [Fact]
        public async void When_PostingWithoutAction_Should_Return405()
        {
            var result = await _restApiClient.Post("/api/winservices/WinRM");
            Assert.Equal(HttpStatusCode.MethodNotAllowed, result.Response.StatusCode);
        }

        [Fact]
        public async void When_PostingUnknownAction_Should_Return404()
        {
            var result = await _restApiClient.Post("/api/winservices/WinRM/unknown");
            Assert.Equal(HttpStatusCode.NotFound, result.Response.StatusCode);
        }

        [Fact]
        public async void When_StartingNonExistingService_Should_Return500AndExceptionMessage()
        {
            var result = await _restApiClient.Post("/api/winservices/not-existing-service-name/start");
            var jcontent = JParser.ParseContent(result.Content);

            Assert.Equal(HttpStatusCode.InternalServerError, result.Response.StatusCode);
            Assert.Equal("Servant.Exceptions.ServantException", jcontent["ExceptionType"]);
            Assert.NotNull(jcontent["ExceptionMessage"]);
        }

        [Fact]
        public async void When_StartingStoppedService_Should_StartService()
        {
            var serviceName = "WinRM";
            await EnsureServiceIsNotDisabled(serviceName);
            EnsureServiceIsStopped(serviceName);

            var result = await _restApiClient.Post($"/api/winservices/{serviceName}/start");

            var serviceCtrl = new ServiceController(serviceName);
            Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.Equal(ServiceControllerStatus.Running, serviceCtrl.Status);
        }

        [Fact]
        public async void When_StartingRunningService_Should_Return500AndExceptionMessage()
        {
            var serviceName = "WinRM";
            await EnsureServiceIsNotDisabled(serviceName);
            EnsureServiceIsRunning(serviceName);

            var result = await _restApiClient.Post($"/api/winservices/{serviceName}/start");
            var jcontent = JParser.ParseContent(result.Content);

            Assert.Equal(HttpStatusCode.InternalServerError, result.Response.StatusCode);
            Assert.Equal("System.InvalidOperationException", jcontent["ExceptionType"]);
            Assert.NotNull(jcontent["ExceptionMessage"]);
        }

        [Fact]
        public async void When_RestartingRunning_Should_RestartService()
        {
            var serviceName = "WinRM";
            await EnsureServiceIsNotDisabled(serviceName);
            EnsureServiceIsRunning(serviceName);

            var result = await _restApiClient.Post($"/api/winservices/{serviceName}/restart");

            var serviceCtrl = new ServiceController(serviceName);
            Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.Equal(ServiceControllerStatus.Running, serviceCtrl.Status);
        }

        [Fact]
        public async void When_RestartingStoppedService_Should_RestartService()
        {
            var serviceName = "WinRM";
            await EnsureServiceIsNotDisabled(serviceName);
            EnsureServiceIsStopped(serviceName);

            var result = await _restApiClient.Post($"/api/winservices/{serviceName}/restart");
            var jcontent = JParser.ParseContent(result.Content);

            Assert.Equal(HttpStatusCode.InternalServerError, result.Response.StatusCode);
            Assert.Equal("System.InvalidOperationException", jcontent["ExceptionType"]);
            Assert.NotNull(jcontent["ExceptionMessage"]);
        }

        [Fact]
        public async void When_StoppingRunningService_Should_StopService()
        {
            var serviceName = "WinRM";
            await EnsureServiceIsNotDisabled(serviceName);
            EnsureServiceIsRunning(serviceName);

            var result = await _restApiClient.Post($"/api/winservices/{serviceName}/stop");

            var serviceCtrl = new ServiceController(serviceName);
            Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.Equal(ServiceControllerStatus.Stopped, serviceCtrl.Status);
        }

        [Fact]
        public async void When_StoppingStoppedService_Should_StopService()
        {
            var serviceName = "WinRM";
            await EnsureServiceIsNotDisabled(serviceName);
            EnsureServiceIsStopped(serviceName);

            var result = await _restApiClient.Post($"/api/winservices/{serviceName}/stop");
            var jcontent = JParser.ParseContent(result.Content);

            Assert.Equal(HttpStatusCode.InternalServerError, result.Response.StatusCode);
            Assert.Equal("System.InvalidOperationException", jcontent["ExceptionType"]);
            Assert.NotNull(jcontent["ExceptionMessage"]);
        }

        [Fact]
        public async void When_SettingStartTypeWithNoValue_Should_Return400AndValidationMessage()
        {
            var result = await _restApiClient.Post("/api/winservices/WinRM/setStartType");
            var jcontent = JParser.ParseContent(result.Content);

            Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
            Assert.Equal("The request is invalid.", jcontent["Message"]);
        }

        [Fact]
        public async void When_SettingStartTypeWithEmptyValue_Should_Return400AndValidationMessage()
        {
            var values = new KeyValueList<string, string> { { "startType", "" } };
            var result = await _restApiClient.Post("/api/winservices/WinRM/setStartType", values);
            var jcontent = JParser.ParseContent(result.Content);

            Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
            Assert.Equal("The request is invalid.", jcontent["Message"]);
        }

        [Fact]
        public async void When_SettingStartTypeWithUnknownValue_Should_Return400AndValidationMessage()
        {
            var values = new KeyValueList<string, string> {{"startType", "unknown"}};
            var result = await _restApiClient.Post("/api/winservices/WinRM/setStartType", values);
            var jcontent = JParser.ParseContent(result.Content);

            Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
            Assert.Equal("The request is invalid.", jcontent["Message"]);
        }

        [Fact]
        public async void When_SettingStartType_Should_SetServiceStartMode()
        {
            var serviceName = "WinRM";
            var values1 = new KeyValueList<string, string> {{"startType", "Automatic"}};
            var values2 = new KeyValueList<string, string> {{"startType", "Disabled"}};
            var values3 = new KeyValueList<string, string> {{"startType", "Manual"}};

            var result1 = await _restApiClient.Post($"/api/winservices/{serviceName}/setStartType", values1);
            var serviceInfo1 = await GetService(serviceName);

            var result2 = await _restApiClient.Post($"/api/winservices/{serviceName}/setStartType", values2);
            var serviceInfo2 = await GetService(serviceName);

            var result3 = await _restApiClient.Post($"/api/winservices/{serviceName}/setStartType", values3);
            var serviceInfo3 = await GetService(serviceName);

            Assert.Equal(HttpStatusCode.OK, result1.Response.StatusCode);
            Assert.Equal(HttpStatusCode.OK, result2.Response.StatusCode);
            Assert.Equal(HttpStatusCode.OK, result3.Response.StatusCode);
            Assert.Equal("Auto", serviceInfo1.StartMode);
            Assert.Equal("Disabled", serviceInfo2.StartMode);
            Assert.Equal("Manual", serviceInfo3.StartMode);
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
                var values = new KeyValueList<string, string> {{"startType", "Automatic"}};
                await _restApiClient.Post($"/api/winservices/{serviceName}/setStartType", values);
            }
        }

        private async Task<WinServiceFullInfoModel> GetService(string serviceName)
        {
            var result = await _restApiClient.Get($"/api/winservices/{serviceName}");
            return JsonConvert.DeserializeObject<WinServiceFullInfoModel>(result.Content);
        }
    }
}
