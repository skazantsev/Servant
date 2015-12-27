using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Servant.Common.Entities;
using Xunit;

namespace Servant.End2EndTests.ApiTests
{
    public partial class FileSystemTests
    {
        [Fact]
        public async void When_GettingDrives_Should_ReturnListOfDriveInfo()
        {
            var result = await _restApiClient.Get("/api/fs/drives");
            var drives = JsonConvert.DeserializeObject<List<DriveInfoModel>>(result.Content);

            Assert.NotNull(drives);
            Assert.True(drives.Any(x => x.Name.Equals("C:\\", StringComparison.OrdinalIgnoreCase)));
            Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
        }
    }
}
