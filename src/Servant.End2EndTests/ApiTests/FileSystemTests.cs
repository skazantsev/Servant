using System;
using Servant.End2EndTests.Core;

namespace Servant.End2EndTests.ApiTests
{
    public partial class FileSystemTests
    {
        private readonly RestApiTestClient _restApiClient;

        public FileSystemTests()
        {
            _restApiClient = new RestApiTestClient(new Uri("http://localhost:8025"));
        }
    }
}
