using System;
using Microsoft.Owin.Hosting;

namespace Servant
{
    public class ServantServiceHost
    {
        private IDisposable _app;

        public void Start()
        {
            _app = WebApp.Start<Startup>("http://localhost:8025");
        }

        public void Stop()
        {
            if (_app != null)
                _app.Dispose();
        }
    }
}
