
namespace SuperSocketLite.SocketBase.Protocol;

/// <summary>
/// The interface for request info parser 
/// </summary>
public interface IRequestInfoParser<TRequestInfo>
    where TRequestInfo : IRequestInfo
{
    /// <summary>
    /// Parses the request info from the source string.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    TRequestInfo ParseRequestInfo(string source);
}
