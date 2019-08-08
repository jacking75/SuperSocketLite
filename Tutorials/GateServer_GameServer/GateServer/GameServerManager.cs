using SuperSocket.SocketBase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GateServer
{
    class GameServerManager
    {
        bool IsRunning = false;

        Func<IPEndPoint, string> ConnectTo;

        List<GameServer> GameServerList = new List<GameServer>();

        ConcurrentBag<string> DisConnectedSessionIDQueue = new ConcurrentBag<string>();
        System.Threading.Thread WorkThread = null;

        public void Init(List<IPEndPoint> addressList, Func<IPEndPoint, string> connectToFunc)
        {
            ConnectTo = connectToFunc;

            foreach(var address in addressList)
            {
                var server = new GameServer();
                server.Init(address);

                GameServerList.Add(server);
            }
        }

        public bool Start()
        {
            IsRunning = true;

            foreach (var server in GameServerList)
            {
                if(ConnectToGameServer(server) == false)
                {
                    return false;
                }                
            }

            WorkThread = new System.Threading.Thread(this.Update);
            WorkThread.Start();
            return true;
        }

        public void Stop()
        {
            IsRunning = false;

            if(WorkThread != null && WorkThread.IsAlive)
            {
                WorkThread.Join();
            }            
        }

        public void DisConnectedServer(string sessionID)
        {
            DisConnectedSessionIDQueue.Add(sessionID);
        }

        void Update()
        {
            while (IsRunning)
            {
                System.Threading.Thread.Sleep(1000);

                if(DisConnectedSessionIDQueue.TryTake(out var sessionID))
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

                        DisConnectedSessionIDQueue.Add(sessionID);
                        continue;
                    }
                }                
            }
        }

        GameServer GetGameServer(string sessionId)
        {
            return GameServerList.Find(x => x.NetSessionID == sessionId);
        }

        bool ConnectToGameServer(GameServer server)
        {
            var sessionID = ConnectTo(server.EndPoint);

            if (sessionID.IsNullOrEmpty())
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
}
