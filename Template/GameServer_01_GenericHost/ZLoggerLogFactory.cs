using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace GameServer_01_GenericHost;


public class SuperSocketLogProvider : SuperSocketLite.SocketBase.Logging.ILogFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _configuration;

    public SuperSocketLogProvider(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _loggerFactory = loggerFactory;
        _configuration = configuration;
    }

    public SuperSocketLite.SocketBase.Logging.ILog GetLog(string name)
    {
        var logger = _loggerFactory.CreateLogger(name);
        return new ZLoggerLog(logger);
    }
}
