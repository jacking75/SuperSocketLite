using System;


namespace PvPGameServer;

public class NLogLog : SuperSocketLite.SocketBase.Logging.ILog
{
    private NLog.ILogger _logger;


    public NLogLog(NLog.ILogger logger)
    {
        if (logger == null)
        {
            throw new ArgumentNullException("log");
        }

        _logger = logger;
    }

    public bool IsDebugEnabled
    {
        get { return _logger.IsDebugEnabled; }
    }

    public bool IsErrorEnabled
    {
        get { return _logger.IsErrorEnabled; }
    }

    public bool IsFatalEnabled
    {
        get { return _logger.IsFatalEnabled; }
    }

    public bool IsInfoEnabled
    {
        get { return _logger.IsInfoEnabled; }
    }

    public bool IsWarnEnabled
    {
        get { return _logger.IsWarnEnabled; }
    }

    public void Debug(string message)
    {
        _logger.Debug(message);
    }                         
    
    public void Error(string message)
    {
        _logger.Error(message);
    }
    
    public void Error(string message, Exception exception)
    {
        _logger.Error($"msg:{message}, exception:{exception.ToString()}");
    }
            
    public void Fatal(string message)
    {
        _logger.Fatal(message);
    }
    
    public void Fatal(string message, Exception exception)
    {
        _logger.Fatal($"msg:{message}, exception:{exception.ToString()}");
    }
          
    public void Info(string message)
    {
        _logger.Info(message);
    }
             
    public void Warn(string message)
    {
        _logger.Warn(message);
    }

    
    
}
