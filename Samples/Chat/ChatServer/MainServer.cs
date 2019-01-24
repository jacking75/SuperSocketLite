using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;

using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine;

using CSBaseLib;
using CommonServerLib;


//TODO 1. 주기적으로 접속한 세션이 패킷을 주고 받았는지 조사(좀비 클라이언트 검사)

namespace ChatServer
{
    public class MainServer : AppServer<ClientSession, EFBinaryRequestInfo>
    {
        static SuperSocket.SocketBase.Logging.ILog MainLogger; 
        IBootstrap ActiveServerBootstrap;
        static RemoteConnectCheck RemoteCheck = null;
        PacketDistributor Distributor = new PacketDistributor();

        
        public MainServer()
            : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
        {
            NewSessionConnected += new SessionHandler<ClientSession>(OnConnected);
            SessionClosed += new SessionHandler<ClientSession, CloseReason>(OnClosed);
            NewRequestReceived += new RequestHandler<ClientSession, EFBinaryRequestInfo>(OnPacketReceived);
        }

        public void CreateStartServer()
        {
            ActiveServerBootstrap = BootstrapFactory.CreateBootstrap();

            if (!ActiveServerBootstrap.Initialize())
            {
                Console.WriteLine(string.Format("서버 초기화 실패"), LOG_LEVEL.ERROR);
                return;
            }
            else
            {
                var refAppServer = ActiveServerBootstrap.AppServers.FirstOrDefault() as MainServer;
                MainLogger = refAppServer.Logger;
                WriteLog("서버 초기화 성공", LOG_LEVEL.INFO);
            }


            var result = ActiveServerBootstrap.Start();

            if (result != StartResult.Success)
            {
                MainServer.WriteLog(string.Format("서버 시작 실패"), LOG_LEVEL.ERROR);
                return;
            }
            else
            {
                WriteLog("서버 시작 성공", LOG_LEVEL.INFO);
            }

            WriteLog(string.Format("서버 생성 및 시작 성공"), LOG_LEVEL.INFO);

            
            ChatServerEnvironment.Setting();
                        
            StartRemoteConnect();

            var appServer = ActiveServerBootstrap.AppServers.FirstOrDefault() as MainServer;
            InnerMessageHostProgram.ServerStart(ChatServerEnvironment.ChatServerUniqueID, appServer.Config.Port);

            ClientSession.CreateIndexPool(appServer.Config.MaxConnectionNumber);            
        }

        public void StartRemoteConnect()
        {
            RemoteCheck = new RemoteConnectCheck();

            var remoteInfoList = new List<Tuple<string, string, int>>();

            foreach(var server in ConfigTemp.RemoteServers)
            {
                var infoList = server.Split(":");
                remoteInfoList.Add(new Tuple<string, string, int>(infoList[0], infoList[1], infoList[2].ToInt32()));

                MainServer.WriteLog(string.Format("(To)연결할 서버 정보: {0}, {1}, {2}", infoList[0], infoList[1], infoList[2]), LOG_LEVEL.INFO);
            }

            RemoteCheck.Init(ActiveServerBootstrap, remoteInfoList);
        }

        public void StopServer()
        {
            var appServer = ActiveServerBootstrap.AppServers.FirstOrDefault() as MainServer;

            RemoteCheck.Stop();
            appServer.Stop();

            appServer.Distributor.Destory();
        }

        public ERROR_CODE CreateComponent()
        {
            var appServer = ActiveServerBootstrap.AppServers.FirstOrDefault() as MainServer;
            var error = appServer.Distributor.Create(appServer);

            if (error != ERROR_CODE.NONE)
            {
                return error;
            }

            InnerMessageHostProgram.CreateComponent();

            return ERROR_CODE.NONE;
        }

        //TODO TimeOut을 3초로 잡고, 상대방이 3초동안 receive를 하지 않아도 send에 문제가 없는지 알아본다.
        public bool SendData(string sessionID, byte[] sendData)
        {
            try
            {
                var session = GetSessionByID(sessionID);

                if (session == null)
                {
                    return false;
                }

                session.Send(sendData, 0, sendData.Length);
            }
            catch(Exception)
            {
                //TODO send time out 등의 문제이므로 접속을 끊는 것이 좋다.
                //session.SendEndWhenSendingTimeOut(); 
                //session.Close();
            }
            return true;
        }

        public PacketDistributor GetPacketDistributor() { return Distributor; }

        public static void WriteLog(string msg, LOG_LEVEL logLevel = LOG_LEVEL.DEBUG)
        {
            switch (logLevel)
            {
                case LOG_LEVEL.INFO:
                    MainLogger.Info(msg);
                    break;
                case LOG_LEVEL.ERROR:
                    MainLogger.Error(msg);
                    break;
                case LOG_LEVEL.DEBUG:
                    MainLogger.Debug(msg);
                    break;
                case LOG_LEVEL.WARN:
                    MainLogger.Warn(msg);
                    break;
                case LOG_LEVEL.FATAL:
                    MainLogger.Fatal(msg);
                    break;
             }
        }
                
        void OnConnected(ClientSession session)
        {
            //옵션의 최대 연결 수를 넘으면 SuperSocket이 바로 접속을 짤라버린다. 즉 이 OnConneted 함수가 호출되지 않는다

            session.AllocSessionIndex();
            WriteLog(string.Format("세션 번호 {0} 접속", session.SessionID), LOG_LEVEL.INFO);
                        
            var packet = ServerPacketData.MakeNTFInConnectOrDisConnectClientPacket(true, session.SessionID, session.SessionIndex);            
            Distributor.DistributeCommon(false, packet);
        }

        void OnClosed(ClientSession session, CloseReason reason)
        {
            WriteLog(string.Format("세션 번호 {0} 접속해제: {1}", session.SessionID, reason.ToString()), LOG_LEVEL.INFO);


            var packet = ServerPacketData.MakeNTFInConnectOrDisConnectClientPacket(false, session.SessionID, session.SessionIndex);
            Distributor.DistributeCommon(false, packet);

            session.FreeSessionIndex(session.SessionIndex);
        }

        void OnPacketReceived(ClientSession session, EFBinaryRequestInfo reqInfo)
        {
            WriteLog(string.Format("세션 번호 {0} 받은 데이터 크기: {1}, ThreadId: {2}", session.SessionID, reqInfo.Body.Length, System.Threading.Thread.CurrentThread.ManagedThreadId), LOG_LEVEL.DEBUG);

            var packet = new ServerPacketData();
            packet.SessionID = session.SessionID;
            packet.SessionIndex = session.SessionIndex;
            packet.PacketSize = reqInfo.Size;            
            packet.PacketID = reqInfo.PacketID;
            packet.Type = reqInfo.Type;
            packet.BodyData = reqInfo.Body;
                    
            Distributor.Distribute(packet);
        }
    }

    class ConfigTemp
    {
        static public List<string> RemoteServers = new List<string>();
    }
}
