using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SuperSocketLite.SocketBase;
using SuperSocketLite.SocketBase.Protocol;
using SuperSocketLite.SocketBase.Config;



namespace GameServer_01_GenericHost;


class MainServer : AppServer<NetworkSession, PacketRequestInfo>, IHostedService
{
    Dictionary<int, Action<NetworkSession, PacketRequestInfo>> HandlerMap = new Dictionary<int, Action<NetworkSession, PacketRequestInfo>>();
    CommonHandler CommonHan = new CommonHandler();

    ServerOption ServerOpt;
    IServerConfig m_Config;

    private readonly IHostApplicationLifetime AppLifetime;
    private readonly ILogger<MainServer> AppLogger;
    private readonly SuperSocketLite.SocketBase.Logging.ILogFactory _logFactory;

    public MainServer(IHostApplicationLifetime appLifetime, 
                    IOptions<ServerOption> serverConfig, 
                    ILogger<MainServer> logger,
                    SuperSocketLite.SocketBase.Logging.ILogFactory logFactory)
        : base(new DefaultReceiveFilterFactory<PacketReceiveFilter, PacketRequestInfo>())
    {
        ServerOpt = serverConfig.Value;
        AppLogger = logger;
        AppLifetime = appLifetime;
        _logFactory = logFactory;

        NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
        SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
        NewRequestReceived += new RequestHandler<NetworkSession, PacketRequestInfo>(RequestReceived);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        AppLifetime.ApplicationStarted.Register(AppOnStarted);
        AppLifetime.ApplicationStarted.Register(AppOnStopped);
                    
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void AppOnStarted()
    {
        //AppLogger.LogInformation("OnStarted");

        InitConfig(ServerOpt);

        CreateServer();

        var IsResult = base.Start();

        if (IsResult)
        {
            AppLogger.LogInformation("서버 네트워크 시작");
        }
        else
        {
            AppLogger.LogError("서버 네트워크 시작 실패");
            return;
        }        
    }

    private void AppOnStopped()
    {
        AppLogger.LogInformation("OnStopped");

        base.Stop();
    }

    void RegistHandler()
    {
        HandlerMap.Add((int)PacketId.ReqEcho, CommonHan.RequestEcho);

        AppLogger.LogInformation("핸들러 등록 완료");
    }

    public void InitConfig(ServerOption option)
    {
        m_Config = new ServerConfig
        {
            Port = option.Port,
            Ip = "Any",
            MaxConnectionNumber = option.MaxConnectionNumber,
            Mode = SocketMode.Tcp,
            Name = option.Name
        };
    }

    public void CreateServer()
    {
        try
        {            
            bool bResult = Setup(new RootConfig(), m_Config, logFactory: _logFactory);

            if (bResult == false)
            {
                AppLogger.LogError("서버 네트워크 설정 실패 ㅠㅠ");
                return;
            }
            
            RegistHandler();

            AppLogger.LogInformation("서버 생성 성공");
        }
        catch(Exception ex)
        {
            AppLogger.LogError($"서버 생성 실패: {ex.ToString()}");
        }
    }

    public bool IsRunning(ServerState eCurState)
    {
        if (eCurState == ServerState.Running)
        {
            return true;
        }

        return false;
    }

    void OnConnected(NetworkSession session)
    {
        AppLogger.LogInformation($"세션 번호 {session.SessionID} 접속");
    }

    void OnClosed(NetworkSession session, CloseReason reason)
    {
        AppLogger.LogInformation($"세션 번호 {session.SessionID} 접속해제: {reason.ToString()}");
    }

    void RequestReceived(NetworkSession session, PacketRequestInfo reqInfo)
    {
        //AppLogger.LogInformation($"세션 번호 {session.SessionID}, 받은 데이터 크기: {reqInfo.Body.Length}");

        var PacketID = reqInfo.PacketID;
        
        if (HandlerMap.ContainsKey(PacketID))
        {
            HandlerMap[PacketID](session, reqInfo);
        }
        else
        {
            AppLogger.LogInformation($"세션 번호 {session.SessionID}, 받은 데이터 크기: {reqInfo.Body.Length}");
        }
    }
}


public class NetworkSession : AppSession<NetworkSession, PacketRequestInfo>
{
}
