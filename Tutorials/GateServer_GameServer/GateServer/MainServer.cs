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


//TODO 1. 주기적으로 접속한 세션이 패킷을 주고 받았는지 조사(좀비 클라이언트 검사)

namespace GateServer
{
    public class MainServer : AppServer<ClientSession, EFBinaryRequestInfo>
    {
        public static ServerOption ServerOption;
        public static SuperSocket.SocketBase.Logging.ILog MainLogger;

        SuperSocket.SocketBase.Config.IServerConfig m_Config;

        PacketProcessor MainPacketProcessor = new PacketProcessor();

        ClientSessionManager ClientSessionMgr = new ClientSessionManager();
        
        
        public MainServer()
            : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
        {
            NewSessionConnected += new SessionHandler<ClientSession>(OnConnected);
            SessionClosed += new SessionHandler<ClientSession, CloseReason>(OnClosed);
            NewRequestReceived += new RequestHandler<ClientSession, EFBinaryRequestInfo>(OnPacketReceived);
        }

        public void InitConfig(ServerOption option)
        {
            ServerOption = option;

            m_Config = new SuperSocket.SocketBase.Config.ServerConfig
            {
                Name = option.Name,
                Ip = "Any",
                Port = option.Port,
                Mode = SocketMode.Tcp,
                MaxConnectionNumber = option.MaxConnectionNumber,
                MaxRequestLength = option.MaxRequestLength,
                ReceiveBufferSize = option.ReceiveBufferSize,
                SendBufferSize = option.SendBufferSize
            };
        }

        public void CreateStartServer()
        {
            try
            {
                bool bResult = Setup(new SuperSocket.SocketBase.Config.RootConfig(), m_Config, logFactory: new NLogLogFactory());

                if (bResult == false)
                {
                    Console.WriteLine("[ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
                    return;
                }
                else
                {
                    MainLogger = base.Logger;
                    MainLogger.Info("서버 초기화 성공");
                }


                CreateComponent();

                Start();

                MainLogger.Info("서버 생성 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 서버 생성 실패: {ex.ToString()}");
            }          
        }

        
        public void StopServer()
        {            
            Stop();

            MainPacketProcessor.Destory();
        }

        public ERROR_CODE CreateComponent()
        {
            ClientSessionMgr.Init(m_Config.MaxConnectionNumber);
                        
            MainPacketProcessor = new PacketProcessor();
            MainPacketProcessor.CreateAndStart(SendData);

            MainLogger.Info("CreateComponent - Success");
            return ERROR_CODE.NONE;
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
            catch(Exception ex)
            {
                // TimeoutException 예외가 발생할 수 있다
                MainServer.MainLogger.Error($"{ex.ToString()},  {ex.StackTrace}");

                session.SendEndWhenSendingTimeOut(); 
                session.Close();
            }
        }

        public void Distribute(ServerPacketData requestPacket)
        {
            MainPacketProcessor.InsertPacket(requestPacket);
        }
                        
        void OnConnected(ClientSession session)
        {
            //옵션의 최대 연결 수를 넘으면 SuperSocket이 바로 접속을 짤라버린다. 즉 이 OnConneted 함수가 호출되지 않는다

            if(ClientSessionMgr.NewSession(session) == false)
            {
                //TODO 최대 접속 수를 넘었으니 클라이언트에게 연결을 끊어라고 통보한다.

                MainLogger.Info(string.Format("세션 번호 {0} 접속", session.SessionID));
                return;
            }

            MainLogger.Info(string.Format("세션 번호 {0} 접속", session.SessionID));
                        
            //var packet = ServerPacketData.MakeNTFInConnectOrDisConnectClientPacket(true, session.SessionID, session.SessionIndex);            
            //Distribute(packet);
        }

        void OnClosed(ClientSession session, CloseReason reason)
        {
            MainLogger.Info(string.Format("세션 번호 {0} 접속해제: {1}", session.SessionID, reason.ToString()));

            //TODO:로그인을 성공한 경우라면 게임서버에 통보해야 한다.
            
            ClientSessionMgr.ColesdSession(session);
        }

        void OnPacketReceived(ClientSession session, EFBinaryRequestInfo reqInfo)
        {
            MainLogger.Debug(string.Format("세션 번호 {0} 받은 데이터 크기: {1}, ThreadId: {2}", session.SessionID, reqInfo.Body.Length, System.Threading.Thread.CurrentThread.ManagedThreadId));

            if(reqInfo.PacketID == (Int16)PACKETID.REQ_LOGIN)
            {
                RequestLogin(session.SessionID, session.SessionIndex, reqInfo.Body);
                return;
            }


            var packet = new ServerPacketData();
            packet.SessionID = session.SessionID;
            packet.SessionIndex = session.SessionIndex;
            packet.PacketSize = reqInfo.Size;            
            packet.PacketID = reqInfo.PacketID;
            packet.Type = reqInfo.Type;
            packet.BodyData = reqInfo.Body;
                    
            Distribute(packet);
        }

        public void RequestLogin(string sessionID, Int32 sessionIndex, byte[] bodyData)
        {
            MainServer.MainLogger.Debug("로그인 요청 받음");

            try
            {
                //TODO 로그인 처리하기

                MainServer.MainLogger.Debug("로그인 요청 답변 보냄");

            }
            catch (Exception ex)
            {
                // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
                MainServer.MainLogger.Error(ex.ToString());
            }
        }
    }

   
}
