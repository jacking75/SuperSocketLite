using SuperSocketLite.SocketBase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GateServer;

class GameServerManager
{
    bool _isRunning = false;

    Func<IPEndPoint, string> ConnectToFunc;

    List<GameServer> _gameServerList = new ();

    ConcurrentBag<string> _disConnectedSessionIDQueue = new ();

    System.Threading.Thread _workThread = null;


    public void Init(List<IPEndPoint> addressList, Func<IPEndPoint, string> connectToFunc)
    {
        ConnectToFunc = connectToFunc;

        foreach(var address in addressList)
        {
            var server = new GameServer();
            server.Init(address);

            _gameServerList.Add(server);
        }
    }

    public bool Start()
    {
        _isRunning = true;

        foreach (var server in _gameServerList)
        {
            if(ConnectToGameServer(server) == false)
            {
                return false;
            }                
        }

        _workThread = new System.Threading.Thread(this.Update);
        _workThread.Start();
        return true;
    }

    public void Stop()
    {
        _isRunning = false;

        if(_workThread != null && _workThread.IsAlive)
        {
            _workThread.Join();
        }            
    }

    public void DisConnectedServer(string sessionID)
    {
        _disConnectedSessionIDQueue.Add(sessionID);
    }

    void Update()
    {
        while (_isRunning)
        {
            System.Threading.Thread.Sleep(1000);

            if(_disConnectedSessionIDQueue.TryTake(out var sessionID))
            {
                var server = GetGameServer(sessionID);
                if(server == null)
                {
                    //TODO 로그 남겨야 할 듯
                    continue;
                }

                if (ConnectToGameServer(server) == false)
                {
                    //TODO 로그 남겨야 할 듯

                    _disConnectedSessionIDQueue.Add(sessionID);
                    continue;
                }
            }                
        }
    }

    GameServer GetGameServer(string sessionId)
    {
        return _gameServerList.Find(x => x.NetSessionID == sessionId);
    }

    bool ConnectToGameServer(GameServer server)
    {
        var sessionID = ConnectToFunc(server.EndPoint);

        if (string.IsNullOrEmpty(sessionID))
        {
            return false;
        }

        server.SetConnected(sessionID);
        return true;
    }
}

class GameServer
{
    public string NetSessionID { get; private set; }
    public IPEndPoint EndPoint { get; private set; }

    public void Init(IPEndPoint endPoint)
    {
        EndPoint = endPoint;
    }

    public void SetConnected(string sessionID)
    {
        NetSessionID = sessionID;
    }

    public void SetDisConnected()
    {
        NetSessionID = "";
    }
}
