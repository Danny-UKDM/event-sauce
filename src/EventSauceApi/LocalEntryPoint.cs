using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace EventSauceApi
{
    public class LocalEntryPoint
    {
        public static void Main(string[] args) => BuildWebHost(args).Run();

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
