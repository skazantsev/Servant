using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Servant.End2EndTests.Core
{
    public class RestApiTestClient
    {
        private readonly Uri _baseAddress;

        private readonly HttpClient _httpClient;

        public RestApiTestClient(Uri baseAddress)
        {
            _baseAddress = baseAddress;
            _httpClient = ConfigureHttpClient();
        }

        public async Task<HttpResponse> Get(string requestUri)
        {
            var response = await _httpClient.GetAsync(requestUri);
            var content = await response.Content.ReadAsStringAsync();
            return new HttpResponse { Content = content, Response = response };
        }

        public async Task<HttpResponse> Post(string requestUri, IEnumerable<KeyValuePair<string, string>> values)
        {
            var response = await _httpClient.PostAsync(requestUri, new FormUrlEncodedContent(values));
            var content = await response.Content.ReadAsStringAsync();
            return new HttpResponse { Content = content, Response = response };
        }

        private HttpClient ConfigureHttpClient()
        {
            return new HttpClient
            {
                BaseAddress = _baseAddress
            };
        }
    }
}
