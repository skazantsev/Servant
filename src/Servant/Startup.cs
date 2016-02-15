using System;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using Newtonsoft.Json;
using Owin;

namespace Servant
{
    [Route]
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = ConfigureApi();
            app.UseWebApi(config);
        }

        private HttpConfiguration ConfigureApi()
        {
            var config = new HttpConfiguration();
            ConfigureRoutes(config);
            ConfigureFormatters(config);
            return config;
        }

        private void ConfigureRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                "FileSystem_Action",
                "api/fs/{action}",
                new { controller = "FileSystem", action = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new { id = RouteParameter.Optional });
        }

        private void ConfigureFormatters(HttpConfiguration config)
        {
            config.Formatters.Add(new TextHtmlJsonFormatter());
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
        }
    }

    public class TextHtmlJsonFormatter : JsonMediaTypeFormatter
    {
        public TextHtmlJsonFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            SerializerSettings.Formatting = Formatting.Indented;
            SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue("application/json");
        }
    }
}
