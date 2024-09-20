using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.IO;
using System.Threading.Tasks;

namespace EchoServer_GenericHost;

class Program
{
    static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;
                //config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddConsole();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<ServerOption>(hostContext.Configuration.GetSection("ServerOption"));
                services.AddHostedService<MainServer>();
            })
            .Build();

        await host.RunAsync();
    }
}
   
public class ServerOption
{
    public int Port { get; set; }

    public int MaxConnectionNumber { get; set; } = 0;

    public string Name { get; set; }        
}

