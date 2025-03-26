using System;

namespace SuperSocket.SocketBase.Logging;

/// <summary>
/// Console Log
/// </summary>
public class ConsoleLog : ILog
{
    private string m_Name;

    private const string m_MessageTemplate = "{0}-{1}: {2}";

    private const string m_Debug = "DEBUG";

    private const string m_Error = "ERROR";

    private const string m_Fatal = "FATAL";

    private const string m_Info = "INFO";

    private const string m_Warn = "WARN";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleLog"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    public ConsoleLog(string name)
    {
        m_Name = name;
    }

    public bool IsDebugEnabled
    {
        get { return true; }
    }

    public bool IsErrorEnabled
    {
        get { return true; }
    }

    public bool IsFatalEnabled
    {
        get { return true; }
    }

    public bool IsInfoEnabled
    {
        get { return true; }
    }

    public bool IsWarnEnabled
    {
        get { return true; }
    }

    public void Debug(string message)
    {
        Console.WriteLine(m_MessageTemplate, m_Name, m_Debug, message);
    }         
    
    public void Error(string message)
    {
        Console.WriteLine(m_MessageTemplate, m_Name, m_Error, message);
    }

    public void Error(string message, Exception exception)
    {
        Console.WriteLine(m_MessageTemplate, m_Name, m_Error, message + Environment.NewLine + exception.Message + exception.StackTrace);
    }
    
    public void Fatal(string message)
    {
        Console.WriteLine(m_MessageTemplate, m_Name, m_Fatal, message);
    }

    public void Fatal(string message, Exception exception)
    {
        Console.WriteLine(m_MessageTemplate, m_Name, m_Fatal, message + Environment.NewLine + exception.Message + exception.StackTrace);
    }

    public void Info(string message)
    {
        Console.WriteLine(m_MessageTemplate, m_Name, m_Info, message);
    }
                     
    public void Warn(string message)
    {
        Console.WriteLine(m_MessageTemplate, m_Name, m_Warn, message);
    }

  
    
}
