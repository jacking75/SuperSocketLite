using System;
using System.Collections.Generic;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using System.Threading.Tasks;
using System.Threading;

namespace EchoServer_GenericHost;


class MainServer : AppServer<NetworkSession, EFBinaryRequestInfo>, IHostedService
{
    public static SuperSocket.SocketBase.Logging.ILog MainLogger;

    Dictionary<int, Action<NetworkSession, EFBinaryRequestInfo>> HandlerMap = new Dictionary<int, Action<NetworkSession, EFBinaryRequestInfo>>();
    CommonHandler CommonHan = new CommonHandler();

    ServerOption ServerOpt;
    IServerConfig m_Config;

    private readonly IHostApplicationLifetime AppLifetime;
    private readonly ILogger<MainServer> AppLogger;

    public MainServer(IHostApplicationLifetime appLifetime, IOptions<ServerOption> serverConfig, ILogger<MainServer> logger)
        : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
    {
        ServerOpt = serverConfig.Value;
        AppLogger = logger;
        AppLifetime = appLifetime;

        NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
        SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
        NewRequestReceived += new RequestHandler<NetworkSession, EFBinaryRequestInfo>(RequestReceived);
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
        AppLogger.LogInformation("OnStarted");

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
        
        //Console.ReadKey();
    }

    private void AppOnStopped()
    {
        AppLogger.LogInformation("OnStopped");

        base.Stop();
    }

    void RegistHandler()
    {
        HandlerMap.Add((int)PACKETID.REQ_ECHO, CommonHan.RequestEcho);

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
            bool bResult = Setup(new RootConfig(), m_Config, logFactory: new NLogLogFactory());

            if (bResult == false)
            {
                AppLogger.LogError("[ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
                return;
            }
            else
            {
                MainLogger = base.Logger;
            }

            RegistHandler();

            MainLogger.Info("서버 생성 성공");
        }
        catch(Exception ex)
        {
            AppLogger.LogError($"[ERROR] 서버 생성 실패: {ex.ToString()}");
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

    void RequestReceived(NetworkSession session, EFBinaryRequestInfo reqInfo)
    {
        //_logger.LogInformation($"세션 번호 {session.SessionID}, 받은 데이터 크기: {reqInfo.Body.Length}");

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


public class NetworkSession : AppSession<NetworkSession, EFBinaryRequestInfo>
{
}
