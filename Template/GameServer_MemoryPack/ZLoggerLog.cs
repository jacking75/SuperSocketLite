using Microsoft.Extensions.Logging;
using System;
using ZLogger;



namespace GameServer_MemoryPack;
public class ZLoggerLog : SuperSocketLite.SocketBase.Logging.ILog
{
    private ILogger _logger;

    public ZLoggerLog(ILogger logger)
    {
        if (logger == null)
        {
            throw new ArgumentNullException("logger");
        }
        _logger = logger;
    }

    public bool IsDebugEnabled => true;
    public bool IsErrorEnabled => true;
    public bool IsFatalEnabled => true;
    public bool IsInfoEnabled => true;
    public bool IsWarnEnabled => true;

    public void Debug(string message)
    {
        _logger.ZLogDebug($"{message}");
    }

    public void Error(string message)
    {
        _logger.ZLogError($"{message}");
    }

    public void Error(string message, Exception exception)
    {
        _logger.ZLogError($"msg:{message}, exception:{exception}");
    }

    public void Fatal(string message)
    {
        _logger.ZLogCritical($"{message}");
    }

    public void Fatal(string message, Exception exception)
    {
        _logger.ZLogCritical($"msg:{message}, exception:{exception}");
    }

    public void Info(string message)
    {
        _logger.ZLogInformation($"{message}");
    }

    public void Warn(string message)
    {
        _logger.ZLogWarning($"{message}");
    }
}