using System;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using System.Threading.Tasks;
using System.Threading;

namespace PvPGameServer;

public class MainServer : AppServer<NetworkSession, MemoryPackBinaryRequestInfo>, IHostedService
{
    public static ILog MainLogger;
            
    PacketProcessor _packetProcessor = new PacketProcessor();
    RoomManager _roomMgr = new RoomManager();

    ServerOption _serverOpt;
    IServerConfig _networkConfig;

    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<MainServer> _appLogger;

    

    public MainServer(IHostApplicationLifetime appLifetime, IOptions<ServerOption> serverConfig, ILogger<MainServer> logger)
        : base(new DefaultReceiveFilterFactory<ReceiveFilter, MemoryPackBinaryRequestInfo>())
    {
        _serverOpt = serverConfig.Value;
        _appLogger = logger;
        _appLifetime = appLifetime;

        NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
        SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
        NewRequestReceived += new RequestHandler<NetworkSession, MemoryPackBinaryRequestInfo>(OnPacketReceived);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(AppOnStarted);
        _appLifetime.ApplicationStopped.Register(AppOnStopped);
                    
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void AppOnStarted()
    {
        _appLogger.LogInformation("OnStarted");

        InitConfig(_serverOpt);
        
        CreateServer(_serverOpt);

        var IsResult = base.Start();

        if (IsResult)
        {
            _appLogger.LogInformation("서버 네트워크 시작");
        }
        else
        {
            _appLogger.LogError("서버 네트워크 시작 실패");
            return;
        }
    }

    private void AppOnStopped()
    {
        MainLogger.Info("OnStopped - begin");

        base.Stop();

        _packetProcessor.Destory();

        MainLogger.Info("OnStopped - end");
    }
            
    public void InitConfig(ServerOption option)
    {
        _networkConfig = new ServerConfig
        {
            Port = option.Port,
            Ip = "Any",
            MaxConnectionNumber = option.MaxConnectionNumber,
            Mode = SocketMode.Tcp,
            Name = option.Name
        };
    }

    public void CreateServer(ServerOption serverOpt)
    {
        try
        {
            bool bResult = Setup(new RootConfig(), _networkConfig, logFactory: new NLogLogFactory());

            if (bResult == false)
            {
                MainLogger.Error("[ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
                return;
            }
            else
            {
                MainLogger = base.Logger;
            }

            CreateComponent(serverOpt);

            MainLogger.Info("서버 생성 성공");
        }
        catch(Exception ex)
        {
            MainLogger.Error($"[ERROR] 서버 생성 실패: {ex.ToString()}");
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

    public void StopServer()
    {
        Stop();

        _packetProcessor.Destory();
    }

    public ERROR_CODE CreateComponent(ServerOption serverOpt)
    {
        Room.NetSendFunc = this.SendData;
        _roomMgr.CreateRooms(serverOpt);

        _packetProcessor = new PacketProcessor();
        _packetProcessor.NetSendFunc = this.SendData;
        _packetProcessor.CreateAndStart(_roomMgr.GetRoomsList(), serverOpt);

        MainLogger.Info("CreateComponent - Success");
        return ERROR_CODE.NONE;
    }

    public bool SendData(string sessionID, byte[] sendData)
    {
        var session = GetSessionByID(sessionID);

        try
        {
            if (session == null)
            {
                return false;
            }

            session.Send(sendData, 0, sendData.Length);
        }
        catch (Exception ex)
        {
            // TimeoutException 예외가 발생할 수 있다
            MainLogger.Error($"{ex.ToString()},  {ex.StackTrace}");

            session.SendEndWhenSendingTimeOut();
            session.Close();
        }
        return true;
    }

    public void Distribute(MemoryPackBinaryRequestInfo requestPacket)
    {
        _packetProcessor.InsertPacket(requestPacket);
    }

    void OnConnected(NetworkSession session)
    {
        // 옵션의 최대 연결 수를 넘으면 SuperSocket이 바로 접속을 짤라버린다. 즉 이 OnConneted 함수가 호출되지 않는다
        MainLogger.Info($"세션 번호 {session.SessionID} 접속");

        var packet = InnerPakcetMaker.MakeNTFInConnectOrDisConnectClientPacket(true, session.SessionID);
        Distribute(packet);
    }

    void OnClosed(NetworkSession session, CloseReason reason)
    {
        MainLogger.Info($"세션 번호 {session.SessionID} 접속해제: {reason.ToString()}");

        var packet = InnerPakcetMaker.MakeNTFInConnectOrDisConnectClientPacket(false, session.SessionID);
        Distribute(packet);
    }

    void OnPacketReceived(NetworkSession session, MemoryPackBinaryRequestInfo reqInfo)
    {
        MainLogger.Debug($"세션 번호 {session.SessionID} 받은 데이터 크기: {reqInfo.Body.Length}, ThreadId: {Thread.CurrentThread.ManagedThreadId}");

        reqInfo.SessionID = session.SessionID;       
        Distribute(reqInfo);         
    }
}


public class NetworkSession : AppSession<NetworkSession, MemoryPackBinaryRequestInfo>
{
}
