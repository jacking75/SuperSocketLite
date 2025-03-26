using System.Net.Sockets;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine;

interface IAsyncSocketSessionBase : ILoggerProvider
{
    SocketAsyncEventArgsProxy SocketAsyncProxy { get; }
    
    Socket Client { get; }
}

interface IAsyncSocketSession : IAsyncSocketSessionBase
{
    void ProcessReceive(SocketAsyncEventArgs e);
}
