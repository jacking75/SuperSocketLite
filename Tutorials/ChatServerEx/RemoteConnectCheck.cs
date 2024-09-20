using System;
using System.Collections.Generic;

using SuperSocket.SocketBase;


namespace ChatServer;

class RemoteConnectCheck
{
    MainServer _appServer;
    List<RemoteCheckState> _remoteList = new List<RemoteCheckState>();
    
    bool _isCheckRunning = false;
    System.Threading.Thread _checkThread = null;


    public void Init(MainServer appServer, List<Tuple<string, string, int>> remoteInfoList)
    {
        _appServer = appServer;

        foreach (var remoteInfo in remoteInfoList)
        {
            var serverAddress = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(remoteInfo.Item2), remoteInfo.Item3);

            var remote = new RemoteCheckState();
            remote.Init(remoteInfo.Item1, serverAddress);

            _remoteList.Add(remote);
        }

        _isCheckRunning = true;
        _checkThread = new System.Threading.Thread(this.CheckAndConnect);
        _checkThread.Start();
    }

    public void Stop()
    {
        _isCheckRunning = false;

        _checkThread.Join();
    }

    void CheckAndConnect()
    {
        while (_isCheckRunning)
        {
            System.Threading.Thread.Sleep(100);

            foreach (var remote in _remoteList)
            {
                if(remote.IsPass())
                {
                    continue;
                }
                else
                {
                    remote.TryConnect(_appServer);
                }
            }
        }
    }



    class RemoteCheckState
    {
        string _serverType;
        System.Net.IPEndPoint _address;
        IAppSession _session = null;

        bool _isTryConnecting = false;
        DateTime _checkedTime = DateTime.Now;


        public void Init(string serverType, System.Net.IPEndPoint endPoint)
        {
            _serverType = serverType;
            _address = endPoint;
        }

        public bool IsPass()
        {
            if ((_session != null && _session.Connected) || _isTryConnecting)
            {
                return true;
            }

            var curTime = DateTime.Now;
            var diffTime = curTime.Subtract(_checkedTime);

            if (diffTime.Seconds <= 3)
            {
                return true;
            }
            else
            {
                _checkedTime = curTime;
            }

            return false;
        }

        public async void TryConnect(MainServer appServer)
        {
            _isTryConnecting = true;
            var activeConnector = appServer as SuperSocket.SocketBase.IActiveConnector;

            try
            {
                var task = await activeConnector.ActiveConnect(_address);

                if (task.Result)
                {
                    _session = task.Session;
                }
            }
            catch
            {

            }
            finally
            {
                _isTryConnecting = false;
            }
        }
    }
}
