using Topshelf;

namespace Servant
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<ServantServiceHost>(s =>
                {
                    s.ConstructUsing(() => new ServantServiceHost());
                    s.WhenStarted(rs => rs.Start());
                    s.WhenStopped(rs => rs.Stop());
                    s.WhenShutdown(rs => rs.Stop());
                });
                x.RunAsLocalSystem();
                x.StartAutomatically();

                x.SetServiceName("Servant");
                x.SetDisplayName("Servant");
                x.SetDescription("A remote manager for your windows server.");
            });
        }
    }
}
