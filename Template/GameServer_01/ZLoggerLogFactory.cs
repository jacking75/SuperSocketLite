using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using ZLogger;
using ZLogger.Providers;


namespace GameServer_01;

public class ZLoggerLogFactory : SuperSocketLite.SocketBase.Logging.LogFactoryBase
{
    private ILoggerFactory _loggerFactory;
    private string _startTime;

    public ZLoggerLogFactory()
        : this("ZLogger.json")
    {
    }

    public ZLoggerLogFactory(string configFile)
        : base(configFile)
    {
        _startTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        var builder = Host.CreateApplicationBuilder();
        builder.Logging
        .ClearProviders()
        .SetMinimumLevel(LogLevel.Trace)
        .AddZLoggerRollingFile(options =>
        {
            // File name determined by parameters to be rotated
            options.FilePathSelector = (timestamp, sequenceNumber) => $"logs/{_startTime}_{sequenceNumber:000}.log";

            // The period of time for which you want to rotate files at time intervals.
            options.RollingInterval = RollingInterval.Day;

            // Limit of size if you want to rotate by file size. (KB)
            options.RollingSizeKB = 1024;

            options.UseJsonFormatter();
        })
        .AddZLoggerConsole(options =>
        {
            options.UseJsonFormatter();
        });
        
        var host = builder.Build();
        _loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

    }

    public override SuperSocketLite.SocketBase.Logging.ILog GetLog(string name)
    {
        return new ZLoggerLog(_loggerFactory.CreateLogger(name));
    }

    
}
