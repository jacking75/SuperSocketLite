using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;

using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine;

using CommonLib;
using MessagePack;



namespace GateServer
{
    public class GameServerProxy : AppServer<GameServerSession, EFBinaryRequestInfo>
    {
        SuperSocket.SocketBase.Config.IServerConfig m_Config;

        public static SuperSocket.SocketBase.Logging.ILog MainLogger;

        GameServerManager GameServerMgr = new GameServerManager();

        public GameServerProxy()
            : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
        {
            NewSessionConnected += new SessionHandler<GameServerSession>(OnConnected);
            SessionClosed += new SessionHandler<GameServerSession, CloseReason>(OnClosed);
            NewRequestReceived += new RequestHandler<GameServerSession, EFBinaryRequestInfo>(OnPacketReceived);
        }

        public void InitConfig()
        {
            m_Config = new SuperSocket.SocketBase.Config.ServerConfig
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
            GameServerMgr.Init(addressList, ConnetcToServer);
        }

        public void CreateStartServer()
        {
            try
            {
                bool bResult = Setup(new SuperSocket.SocketBase.Config.RootConfig(), m_Config, logFactory: new SuperSocket.SocketBase.Logging.NLogLogFactory());

                if (bResult == false)
                {
                    Console.WriteLine("[GameServer][ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
                    return;
                }
                else
                {
                    MainLogger = base.Logger;
                    MainLogger.Info("[GameServer]서버 초기화 성공");
                }


                CreateComponent();

                Start();

                MainLogger.Info("[GameServer]서버 생성 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameServer][ERROR] 서버 생성 실패: {ex.ToString()}");
            }
        }

        public bool StartAllConnectTo()
        {
            return GameServerMgr.Start();
        }


        public void StopServer()
        {
            Stop();

            GameServerMgr.Stop();
        }

        public ERROR_CODE CreateComponent()
        {
            MainLogger.Info("[GameServer]CreateComponent - Success");
            return ERROR_CODE.NONE;
        }

        public string ConnetcToServer(System.Net.IPEndPoint serverAddress)
        {
            //var serverAddress = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port);

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
                MainLogger.Error($"[GameServer] {ex.Message}");
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
                MainServer.MainLogger.Error($"[GameServer]{ex.ToString()},  {ex.StackTrace}");

                session.SendEndWhenSendingTimeOut();
                session.Close();
            }
        }


        void OnConnected(GameServerSession session)
        {
            MainLogger.Info(string.Format("[GameServer]세션 번호 {0} 접속", session.SessionID));

        }

        void OnClosed(GameServerSession session, CloseReason reason)
        {
            MainLogger.Info(string.Format("[GameServer]세션 번호 {0} 접속해제: {1}", session.SessionID, reason.ToString()));

            GameServerMgr.DisConnectedServer(session.SessionID);

        }

        void OnPacketReceived(GameServerSession session, EFBinaryRequestInfo reqInfo)
        {
            MainLogger.Debug(string.Format("[GameServer]세션 번호 {0} 받은 데이터 크기: {1}, ThreadId: {2}", session.SessionID, reqInfo.Body.Length, System.Threading.Thread.CurrentThread.ManagedThreadId));
        }

    }



    public class GameServerSession : AppSession<GameServerSession, EFBinaryRequestInfo>
    {
    }
}
