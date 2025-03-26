using System;

namespace SuperSocket.SocketBase.Logging;

/// <summary>
/// Log interface
/// </summary>
public interface ILog
{
    bool IsDebugEnabled { get; }

    bool IsErrorEnabled { get; }

    bool IsFatalEnabled { get; }

    bool IsInfoEnabled { get; }

    bool IsWarnEnabled { get; }

    void Debug(string message);
            
    void Error(string message);

    void Error(string message, Exception exception);
                    
    void Fatal(string message);

    void Fatal(string message, Exception exception);
            
    void Info(string message);
                    
    void Warn(string message);


    
}
