using Owin;
using System.Web.Http;

namespace Servant
{
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
            config.Routes.MapHttpRoute(
                "WinService",
                "api/winservices/{serviceName}",
                new { controller = "WinServices" });

            config.Routes.MapHttpRoute(
                "FileSystem",
                "api/fs/{action}",
                new { controller = "FileSystem" });

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new { id = RouteParameter.Optional });
            return config;
        }
    }
}
