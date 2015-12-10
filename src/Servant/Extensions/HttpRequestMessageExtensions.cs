using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Servant.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static Dictionary<string, string> QueryString(this HttpRequestMessage request)
        {
            return request.GetQueryNameValuePairs()
                          .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
