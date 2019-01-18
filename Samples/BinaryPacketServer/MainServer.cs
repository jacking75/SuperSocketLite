using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;
using System.Diagnostics;

using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;

namespace BinaryPacketServer
{
    class MainServer : AppServer<NetworkSession, EFBinaryRequestInfo>
    {
        Dictionary<int, Action<NetworkSession, EFBinaryRequestInfo>> HandlerMap = new Dictionary<int, Action<NetworkSession, EFBinaryRequestInfo>>();
        CommonHandler CommonHan = new CommonHandler();

        IServerConfig m_Config;


        public MainServer()
            : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
        {
            NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
            SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
            NewRequestReceived += new RequestHandler<NetworkSession, EFBinaryRequestInfo>(RequestReceived);
        }


        void RegistHandler()
        {
            HandlerMap.Add((int)PACKETID.REQ_ECHO, CommonHan.RequestEcho);

            DevLog.Write(string.Format("핸들러 등록 완료"), LOG_LEVEL.INFO);
        }

        public void InitConfig()
        {
            m_Config = new ServerConfig
            {
                Port = 18732,
                Ip = "Any",
                MaxConnectionNumber = 100,
                Mode = SocketMode.Tcp,
                Name = "BoardServerNet"
            };
        }

        public void CreateServer()
        {
            //TODO NLog로 변경하기
            bool bResult = Setup(new RootConfig(), m_Config, logFactory: new ConsoleLogFactory());

            if (bResult == false)
            {
                DevLog.Write(string.Format("[ERROR] 서버 네트워크 설정 실패 ㅠㅠ"), LOG_LEVEL.ERROR);
                return;
            }

            RegistHandler();

            DevLog.Write(string.Format("서버 생성 성공"), LOG_LEVEL.INFO);
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
            DevLog.Write(string.Format("세션 번호 {0} 접속", session.SessionID), LOG_LEVEL.INFO);
        }

        void OnClosed(NetworkSession session, CloseReason reason)
        {
            DevLog.Write(string.Format("세션 번호 {0} 접속해제: {1}", session.SessionID, reason.ToString()), LOG_LEVEL.INFO);
        }

        void RequestReceived(NetworkSession session, EFBinaryRequestInfo reqInfo)
        {
            DevLog.Write(string.Format("세션 번호 {0} 받은 데이터 크기: {1}, ThreadId: {2}", session.SessionID, reqInfo.Body.Length, System.Threading.Thread.CurrentThread.ManagedThreadId), LOG_LEVEL.INFO);
            
            var PacketID = reqInfo.PacketID;
            var value1 = reqInfo.Value1;
            var value2 = reqInfo.Value2;

            if (HandlerMap.ContainsKey(PacketID))
            {
                HandlerMap[PacketID](session, reqInfo);
            }
            else
            {
                DevLog.Write(string.Format("세션 번호 {0} 받은 데이터 크기: {1}", session.SessionID, reqInfo.Body.Length), LOG_LEVEL.INFO);
            }
        }
    }


    public class NetworkSession : AppSession<NetworkSession, EFBinaryRequestInfo>
    {
    }
}
