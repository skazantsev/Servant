using Newtonsoft.Json.Linq;

namespace Servant.End2EndTests.Helpers
{
    public static class JParser
    {
        public static JObject ParseContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return new JObject();
            try
            {
                return JObject.Parse(content);
            }
            catch
            {
                return new JObject();
            }
        }
    }
}
