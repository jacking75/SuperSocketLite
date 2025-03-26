using System;
using System.Net.Sockets;
using SuperSocket.SocketBase;


namespace SuperSocket.SocketEngine;

class SocketAsyncEventArgsProxy
{
    public SocketAsyncEventArgs SocketEventArgs { get; private set; }

    public int OrigOffset { get; private set; }

    public bool IsRecyclable { get; private set; }

    private SocketAsyncEventArgsProxy()
    {

    }

    public SocketAsyncEventArgsProxy(SocketAsyncEventArgs socketEventArgs)
        : this(socketEventArgs, true)
    {
        
    }

    public SocketAsyncEventArgsProxy(SocketAsyncEventArgs socketEventArgs, bool isRecyclable)
    {
        SocketEventArgs = socketEventArgs;
        OrigOffset = socketEventArgs.Offset;
        SocketEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArgs_Completed);
        IsRecyclable = isRecyclable;
    }

    static void SocketEventArgs_Completed(object sender, SocketAsyncEventArgs e)
    {
        var socketSession = e.UserToken as IAsyncSocketSession;

        if (socketSession == null)
            return;

        if (e.LastOperation == SocketAsyncOperation.Receive)
        {
            socketSession.AsyncRun(() => socketSession.ProcessReceive(e));
        }
        else
        {
            throw new ArgumentException("The last operation completed on the socket was not a receive");
        }
    } 

    public void Initialize(IAsyncSocketSession socketSession)
    {
        SocketEventArgs.UserToken = socketSession;
    }

    public void Reset()
    {
        SocketEventArgs.UserToken = null;
    }
}
