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

namespace PvPGameServer
{
    public class MainServer : AppServer<NetworkSession, EFBinaryRequestInfo>, IHostedService
    {
        public static ILog MainLogger;
                
        PacketProcessor MainPacketProcessor = new PacketProcessor();
        RoomManager RoomMgr = new RoomManager();

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
            NewRequestReceived += new RequestHandler<NetworkSession, EFBinaryRequestInfo>(OnPacketReceived);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            AppLifetime.ApplicationStarted.Register(AppOnStarted);
            AppLifetime.ApplicationStopped.Register(AppOnStopped);
                        
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
            
            CreateServer(ServerOpt);

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
            MainLogger.Info("OnStopped - begin");

            base.Stop();

            MainPacketProcessor.Destory();

            MainLogger.Info("OnStopped - end");
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

        public void CreateServer(ServerOption serverOpt)
        {
            try
            {
                bool bResult = Setup(new RootConfig(), m_Config, logFactory: new NLogLogFactory());

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

            MainPacketProcessor.Destory();
        }

        public ERROR_CODE CreateComponent(ServerOption serverOpt)
        {
            Room.NetSendFunc = this.SendData;
            RoomMgr.CreateRooms(serverOpt);

            MainPacketProcessor = new PacketProcessor();
            MainPacketProcessor.NetSendFunc = this.SendData;
            MainPacketProcessor.DistributePacket = this.Distribute;
            MainPacketProcessor.CreateAndStart(RoomMgr.GetRoomsList(), serverOpt);

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

        public void Distribute(EFBinaryRequestInfo requestPacket)
        {
            MainPacketProcessor.InsertPacket(requestPacket);
        }

        void OnConnected(NetworkSession session)
        {
            // 옵션의 최대 연결 수를 넘으면 SuperSocket이 바로 접속을 짤라버린다. 즉 이 OnConneted 함수가 호출되지 않는다
            MainLogger.Info(string.Format("세션 번호 {0} 접속", session.SessionID));

            var packet = ServerPacketData.MakeNTFInConnectOrDisConnectClientPacket(true, session.SessionID);
            Distribute(packet);
        }

        void OnClosed(NetworkSession session, CloseReason reason)
        {
            MainLogger.Info(string.Format("세션 번호 {0} 접속해제: {1}", session.SessionID, reason.ToString()));

            var packet = ServerPacketData.MakeNTFInConnectOrDisConnectClientPacket(false, session.SessionID);
            Distribute(packet);
        }

        void OnPacketReceived(NetworkSession session, EFBinaryRequestInfo reqInfo)
        {
            MainLogger.Debug(string.Format("세션 번호 {0} 받은 데이터 크기: {1}, ThreadId: {2}", session.SessionID, reqInfo.Body.Length, System.Threading.Thread.CurrentThread.ManagedThreadId));

            reqInfo.SessionID = session.SessionID;       
            Distribute(reqInfo);         
        }
    }


    public class NetworkSession : AppSession<NetworkSession, EFBinaryRequestInfo>
    {
    }
}
