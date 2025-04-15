using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

using ZLogger;
using ZLogger.Providers;


namespace GameServer_01_GenericHost;

class Program
{
    static async Task Main(string[] args)
    {
        var startTime = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        var host = new HostBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;
                //config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureLogging((hostContext, loggingBuilder) =>
            {
                var loggingSection = hostContext.Configuration.GetSection("Logging");
                var zLoggerSection = loggingSection.GetSection("ZLogger");

                loggingBuilder.ClearProviders();
                
                //loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddConfiguration(loggingSection.GetSection("LogLevel"));

                loggingBuilder.AddZLoggerRollingFile(options =>
                {
                    options.FilePathSelector = (timestamp, sequenceNumber) =>
                        $"logs/{startTime}_{sequenceNumber:000}.log";

                    if (Enum.TryParse<RollingInterval>(zLoggerSection["RollingInterval"], out var rollingInterval))
                    {
                        options.RollingInterval = rollingInterval;
                    }
                    else
                    {
                        options.RollingInterval = RollingInterval.Day;
                    }

                    if (int.TryParse(zLoggerSection["RollingSizeKB"], out var rollingSizeKB))
                    {
                        options.RollingSizeKB = rollingSizeKB;
                    }
                    else
                    {
                        options.RollingSizeKB = 1024;
                    }

                    // JSON 포맷터 사용 여부
                    bool useJsonFormatter = true;
                    if (bool.TryParse(zLoggerSection["UseJsonFormatter"], out var jsonFormatter))
                    {
                        useJsonFormatter = jsonFormatter;
                    }

                    if (useJsonFormatter)
                    {
                        options.UseJsonFormatter();
                    }
                });
                loggingBuilder.AddZLoggerConsole(options =>
                {
                    // JSON 포맷터 사용 여부
                    bool useJsonFormatter = true;
                    if (bool.TryParse(zLoggerSection["UseJsonFormatter"], out var jsonFormatter))
                    {
                        useJsonFormatter = jsonFormatter;
                    }

                    if (useJsonFormatter)
                    {
                        options.UseJsonFormatter();
                    }
                });
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<ServerOption>(hostContext.Configuration.GetSection("ServerOption"));
                services.AddHostedService<MainServer>();

                services.AddSingleton<SuperSocketLite.SocketBase.Logging.ILogFactory, SuperSocketLogProvider>();
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

