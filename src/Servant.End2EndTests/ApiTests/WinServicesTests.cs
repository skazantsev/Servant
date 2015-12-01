using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Servant.Common.Entities;
using Servant.End2EndTests.Core;
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
        public async void When_GetWinServices_Should_ReturnServiceNames()
        {
            var result = await _restApiClient.Get("/api/winservices");
            var services = JsonConvert.DeserializeObject<List<WinServiceSimpleInfo>>(result.Content);

            Assert.Equal(200, (int)result.Response.StatusCode);
            Assert.True(services.Count > 0);
            Assert.Contains(services, x => x.ServiceName == "eventlog");
        }
    }
}
