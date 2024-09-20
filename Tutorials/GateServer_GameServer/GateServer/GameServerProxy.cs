using System;
using System.Collections.Generic;

using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

using CommonLib;
using MessagePack;


namespace GateServer;

public class GameServerProxy : AppServer<GameServerSession, EFBinaryRequestInfo>
{
    SuperSocket.SocketBase.Config.IServerConfig _config;

    public static SuperSocket.SocketBase.Logging.ILog s_MainLogger;

    GameServerManager _gameServerMgr = new GameServerManager();


    public GameServerProxy()
        : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
    {
        NewSessionConnected += new SessionHandler<GameServerSession>(OnConnected);
        SessionClosed += new SessionHandler<GameServerSession, CloseReason>(OnClosed);
        NewRequestReceived += new RequestHandler<GameServerSession, EFBinaryRequestInfo>(OnPacketReceived);
    }

    public void InitConfig()
    {
        _config = new SuperSocket.SocketBase.Config.ServerConfig
        {
            Name = "GameServer",
            Ip = "Any",
            Port = 11022,
            Mode = SocketMode.Tcp,
            MaxConnectionNumber = 16,
            MaxRequestLength = 2048,
            ReceiveBufferSize = 2048 * 4,
            SendBufferSize = 2048 * 4
        };


        var serverAddress = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 12021);
        var addressList = new List<System.Net.IPEndPoint>() { serverAddress };
        _gameServerMgr.Init(addressList, ConnetcToServer);
    }

    public void CreateStartServer()
    {
        try
        {
            bool bResult = Setup(new SuperSocket.SocketBase.Config.RootConfig(), _config, logFactory: new NLogLogFactory());

            if (bResult == false)
            {
                Console.WriteLine("[GameServer][ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
                return;
            }
            else
            {
                s_MainLogger = base.Logger;
                s_MainLogger.Info("[GameServer]서버 초기화 성공");
            }


            CreateComponent();

            Start();

            s_MainLogger.Info("[GameServer]서버 생성 성공");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer][ERROR] 서버 생성 실패: {ex.ToString()}");
        }
    }

    public bool StartAllConnectTo()
    {
        return _gameServerMgr.Start();
    }


    public void StopServer()
    {
        Stop();

        _gameServerMgr.Stop();
    }

    public ErrorCode CreateComponent()
    {
        s_MainLogger.Info("[GameServer]CreateComponent - Success");
        return ErrorCode.None;
    }

    public string ConnetcToServer(System.Net.IPEndPoint serverAddress)
    {
        var activeConnector = this as SuperSocket.SocketBase.IActiveConnector;

        try
        {
            var task = activeConnector.ActiveConnect(serverAddress).Result;

            if (task.Result)
            {
                return task.Session.SessionID;
            }
        }
        catch(Exception ex)
        {
            s_MainLogger.Error($"[GameServer] {ex.Message}");
        }

        return "";
    }

    public void SendData(string sessionID, byte[] sendData)
    {
        var session = GetSessionByID(sessionID);

        try
        {
            if (session == null)
            {
                return;
            }

            session.Send(sendData, 0, sendData.Length);
        }
        catch (Exception ex)
        {
            // TimeoutException 예외가 발생할 수 있다
            MainServer.s_MainLogger.Error($"[GameServer]{ex.ToString()},  {ex.StackTrace}");

            session.SendEndWhenSendingTimeOut();
            session.Close();
        }
    }


    void OnConnected(GameServerSession session)
    {
        s_MainLogger.Info(string.Format("[GameServer]세션 번호 {0} 접속", session.SessionID));

    }

    void OnClosed(GameServerSession session, CloseReason reason)
    {
        s_MainLogger.Info(string.Format("[GameServer]세션 번호 {0} 접속해제: {1}", session.SessionID, reason.ToString()));

        _gameServerMgr.DisConnectedServer(session.SessionID);

    }

    void OnPacketReceived(GameServerSession session, EFBinaryRequestInfo reqInfo)
    {
        s_MainLogger.Debug(string.Format("[GameServer]세션 번호 {0} 받은 데이터 크기: {1}, ThreadId: {2}", session.SessionID, reqInfo.Body.Length, System.Threading.Thread.CurrentThread.ManagedThreadId));
    }

}



public class GameServerSession : AppSession<GameServerSession, EFBinaryRequestInfo>
{
}
