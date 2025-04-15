using System;
using System.Net;
using System.Security.Authentication;


namespace SuperSocketLite.SocketBase;

/// <summary>
/// Listener inforamtion
/// </summary>
[Serializable]
public class ListenerInfo
{
    /// <summary>
    /// Gets or sets the listen endpoint.
    /// </summary>
    /// <value>
    /// The end point.
    /// </value>
    public IPEndPoint EndPoint { get; set; }

    /// <summary>
    /// Gets or sets the listen backlog.
    /// </summary>
    /// <value>
    /// The back log.
    /// </value>
    public int BackLog { get; set; }

    /// <summary>
    /// Gets or sets the security protocol.
    /// </summary>
    /// <value>
    /// The security.
    /// </value>
    public SslProtocols Security { get; set; }
}
