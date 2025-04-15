using System.Net.Sockets;
using SuperSocketLite.SocketBase;

namespace SuperSocketLite.SocketEngine;

interface IAsyncSocketSessionBase : ILoggerProvider
{
    SocketAsyncEventArgsProxy SocketAsyncProxy { get; }
    
    Socket Client { get; }
}

interface IAsyncSocketSession : IAsyncSocketSessionBase
{
    void ProcessReceive(SocketAsyncEventArgs e);
}
