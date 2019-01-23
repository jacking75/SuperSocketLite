using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SuperSocket.SocketBase;

namespace ChatServer
{
    class RemoteConnectCheck
    {
        IBootstrap ActiveServerBootstrap;
        List<RemoteCheckState> RemoteList = new List<RemoteCheckState>();
        
        bool IsCheckRunning = false;
        System.Threading.Thread CheckThread = null;


        public void Init(IBootstrap activeServerBootstrap, List<Tuple<string, string, int>> remoteInfoList)
        {
            ActiveServerBootstrap = activeServerBootstrap;

            foreach (var remoteInfo in remoteInfoList)
            {
                var serverAddress = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(remoteInfo.Item2), remoteInfo.Item3);

                var remote = new RemoteCheckState();
                remote.Init(remoteInfo.Item1, serverAddress);

                RemoteList.Add(remote);
            }

            IsCheckRunning = true;
            CheckThread = new System.Threading.Thread(this.CheckAndConnect);
            CheckThread.Start();
        }

        public void Stop()
        {
            IsCheckRunning = false;

            CheckThread.Join();
        }

        void CheckAndConnect()
        {
            while (IsCheckRunning)
            {
                System.Threading.Thread.Sleep(100);

                foreach (var remote in RemoteList)
                {
                    if(remote.IsPass())
                    {
                        continue;
                    }
                    else
                    {
                        remote.TryConnect(ActiveServerBootstrap);
                    }
                }
            }
        }



        class RemoteCheckState
        {
            string ServerType;
            System.Net.IPEndPoint Address;
            IAppSession Session = null;

            bool IsTryConnecting = false;
            DateTime CheckedTime = DateTime.Now;

            public void Init(string serverType, System.Net.IPEndPoint endPoint)
            {
                ServerType = serverType;
                Address = endPoint;
            }

            public bool IsPass()
            {
                if ((Session != null && Session.Connected) || IsTryConnecting)
                {
                    return true;
                }

                var curTime = DateTime.Now;
                var diffTime = curTime.Subtract(CheckedTime);

                if (diffTime.Seconds <= 3)
                {
                    return true;
                }
                else
                {
                    CheckedTime = curTime;
                }

                return false;
            }

            public async void TryConnect(IBootstrap activeServerBootstrap)
            {
                IsTryConnecting = true;

                var appServer = activeServerBootstrap.AppServers.FirstOrDefault() as MainServer;
                var activeConnector = appServer as SuperSocket.SocketBase.IActiveConnector;

                try
                {
                    var task = await activeConnector.ActiveConnect(Address);

                    if (task.Result)
                    {
                        Session = task.Session;
                    }
                }
                catch
                {

                }
                finally
                {
                    IsTryConnecting = false;
                }
            }
        }
    }
}
