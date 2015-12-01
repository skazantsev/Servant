using System.Net.Http;

namespace Servant.End2EndTests.Core
{
    public class HttpResponse
    {
        public string Content { get; set; }

        public HttpResponseMessage Response { get; set; }
    }
}
