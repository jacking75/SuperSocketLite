﻿using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using SuperSocketLite.Common;
using SuperSocketLite.SocketBase;


namespace SuperSocketLite.SocketEngine;

abstract class TcpSocketServerBase : SocketServerBase
{
    private readonly byte[] m_KeepAliveOptionValues;
    private readonly byte[] m_KeepAliveOptionOutValues;
    private readonly int m_SendTimeOut;
    private readonly int m_ReceiveBufferSize;
    private readonly int m_SendBufferSize;
    private readonly bool m_NoDelay;

    public TcpSocketServerBase(IAppServer appServer, ListenerInfo[] listeners)
        : base(appServer, listeners)
    {
        var config = appServer.Config;

        uint dummy = 0;
        m_KeepAliveOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
        m_KeepAliveOptionOutValues = new byte[m_KeepAliveOptionValues.Length];
        //whether enable KeepAlive
        BitConverter.GetBytes((uint)1).CopyTo(m_KeepAliveOptionValues, 0);
        //how long will start first keep alive
        BitConverter.GetBytes((uint)(config.KeepAliveTime * 1000)).CopyTo(m_KeepAliveOptionValues, Marshal.SizeOf(dummy));
        //keep alive interval
        BitConverter.GetBytes((uint)(config.KeepAliveInterval * 1000)).CopyTo(m_KeepAliveOptionValues, Marshal.SizeOf(dummy) * 2);

        m_SendTimeOut = config.SendTimeOut;
        m_ReceiveBufferSize = config.ReceiveBufferSize;
        m_SendBufferSize = config.SendBufferSize;

        m_NoDelay = config.NoDelay;
    }

    protected IAppSession CreateSession(Socket client, ISocketSession session)
    {
        if (m_SendTimeOut > 0)
            client.SendTimeout = m_SendTimeOut;

        if (m_ReceiveBufferSize > 0)
            client.ReceiveBufferSize = m_ReceiveBufferSize;

        if (m_SendBufferSize > 0)
            client.SendBufferSize = m_SendBufferSize;

        if (!Platform.SupportSocketIOControlByCodeEnum)
        {
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, m_KeepAliveOptionValues);
        }
        else
        {
#if WINDOWS
            client.IOControl(IOControlCode.KeepAliveValues, m_KeepAliveOptionValues, m_KeepAliveOptionOutValues);
#endif
        }

        client.NoDelay = m_NoDelay;
        client.LingerState = new LingerOption(enable:false, seconds:0); // socket 종료하면 즉시 제거한다.
        //client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true); //닷넷코어에서 사용 불가

        return this.AppServer.CreateAppSession(session);
    }

    protected override ISocketListener CreateListener(ListenerInfo listenerInfo)
    {
        return new TcpAsyncSocketListener(listenerInfo);
    }
}
