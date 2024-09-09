using System;
using System.Collections.Generic;
using System.Text;

namespace EchoServerEx;

public class NLogLog : SuperSocket.SocketBase.Logging.ILog
{
    private NLog.ILogger _log;

    public NLogLog(NLog.ILogger log)
    {
        if (log == null)
        {
            throw new ArgumentNullException("log");
        }

        _log = log;
    }

    public bool IsDebugEnabled
    {
        get { return _log.IsDebugEnabled; }
    }

    public bool IsErrorEnabled
    {
        get { return _log.IsErrorEnabled; }
    }

    public bool IsFatalEnabled
    {
        get { return _log.IsFatalEnabled; }
    }

    public bool IsInfoEnabled
    {
        get { return _log.IsInfoEnabled; }
    }

    public bool IsWarnEnabled
    {
        get { return _log.IsWarnEnabled; }
    }

    public void Debug(string message)
    {
        _log.Debug(message);
    }                         
    
    public void Error(string message)
    {
        _log.Error(message);
    }
    
    public void Error(string message, Exception exception)
    {
        _log.Error($"msg:{message}, exception:{exception.ToString()}");
    }
            
    public void Fatal(string message)
    {
        _log.Fatal(message);
    }
    
    public void Fatal(string message, Exception exception)
    {
        _log.Fatal($"msg:{message}, exception:{exception.ToString()}");
    }
          
    public void Info(string message)
    {
        _log.Info(message);
    }
             
    public void Warn(string message)
    {
        _log.Warn(message);
    }

    
    
}
