﻿
namespace SuperSocketLite.SocketBase.Logging;

/// <summary>
/// Console log factory
/// </summary>
public class ConsoleLogFactory : ILogFactory
{
    /// <summary>
    /// Gets the log by name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public ILog GetLog(string name)
    {
        return new ConsoleLog(name);
    }
}
