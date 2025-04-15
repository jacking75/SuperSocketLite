﻿using System;
using SuperSocketLite.SocketBase;
using SuperSocketLite.SocketBase.Protocol;
using SuperSocketLite.SocketBase.Config;


namespace SuperSocketLite.SocketEngine;

/// <summary>
/// Default socket server factory
/// </summary>
public class SocketServerFactory : ISocketServerFactory
{
    /// <summary>
    /// Creates the socket server.
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    /// <param name="appServer">The app server.</param>
    /// <param name="listeners">The listeners.</param>
    /// <param name="config">The config.</param>
    /// <returns></returns>
    public ISocketServer CreateSocketServer<TRequestInfo>(IAppServer appServer, ListenerInfo[] listeners, IServerConfig config)
        where TRequestInfo : IRequestInfo
    {
        if (appServer == null)
            throw new ArgumentNullException("appServer");

        if (listeners == null)
            throw new ArgumentNullException("listeners");

        if (config == null)
            throw new ArgumentNullException("config");

        switch(config.Mode)
        {
            case(SocketMode.Tcp):
                return new AsyncSocketServer(appServer, listeners);
            case(SocketMode.Udp):
                return new UdpSocketServer<TRequestInfo>(appServer, listeners);
            default:
                throw new NotSupportedException("Unsupported SocketMode:" + config.Mode);
        }
    }
}
