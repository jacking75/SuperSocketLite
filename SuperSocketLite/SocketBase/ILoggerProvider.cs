using SuperSocketLite.SocketBase.Logging;


namespace SuperSocketLite.SocketBase;

/// <summary>
/// The interface for who provides logger
/// </summary>
public interface ILoggerProvider
{
    /// <summary>
    /// Gets the logger assosiated with this object.
    /// </summary>
    ILog Logger { get; }
}
