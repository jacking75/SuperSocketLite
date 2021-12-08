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
using System.Threading;

namespace EchoServer
{
    class MainServer : AppServer<NetworkSession, EFBinaryRequestInfo>
    {
        public static SuperSocket.SocketBase.Logging.ILog MainLogger;

        //Dictionary<int, Action<NetworkSession, EFBinaryRequestInfo>> HandlerMap = new Dictionary<int, Action<NetworkSession, EFBinaryRequestInfo>>();
        //CommonHandler CommonHan = new CommonHandler();

        IServerConfig m_Config;

        Thread CounterTh;


        public MainServer()
            : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
        {
            NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
            SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
            NewRequestReceived += new RequestHandler<NetworkSession, EFBinaryRequestInfo>(RequestReceived);
        }


        void RegistHandler()
        {
            //HandlerMap.Add((int)PACKETID.REQ_ECHO, CommonHan.RequestEcho);

            MainLogger.Info("핸들러 등록 완료");
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
                bool bResult = Setup(new RootConfig(), m_Config, logFactory: new ConsoleLogFactory());

                if (bResult == false)
                {
                    Console.WriteLine("[ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
                    return;
                }

                MainLogger = base.Logger;

                RegistHandler();

                CounterTh = new Thread(EchoCounter);
                CounterTh.Start();

                MainLogger.Info($"[{DateTime.Now}] 서버 생성 성공");
            }
            catch(Exception ex)
            {
                MainLogger.Error($"서버 생성 실패: {ex.ToString()}");
            }
        }

        Int64 Count = 0;
        void EchoCounter()
        {
            while(true)
            {
                Thread.Sleep(1000);

                var value = Interlocked.Exchange(ref Count, 0);
                //Console.WriteLine($"{DateTime.Now} : {value}");
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
            MainLogger.Debug($"[{DateTime.Now}] 세션 번호 {session.SessionID} 접속 start, ThreadId: {System.Threading.Thread.CurrentThread.ManagedThreadId}");

            //Thread.Sleep(3000);
            //MainLogger.Info($"세션 번호 {session.SessionID} 접속 end, ThreadId: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        }

        void OnClosed(NetworkSession session, CloseReason reason)
        {
            MainLogger.Info($"[{DateTime.Now}] 세션 번호 {session.SessionID},  접속해제: {reason.ToString()}");
        }

        void RequestReceived(NetworkSession session, EFBinaryRequestInfo reqInfo)
        {
            MainLogger.Debug($"[{DateTime.Now}] 세션 번호 {session.SessionID},  받은 데이터 크기: {reqInfo.Body.Length}, ThreadId: {System.Threading.Thread.CurrentThread.ManagedThreadId}");

            Interlocked.Increment(ref Count);
              
            
            var totalSize = (Int16)(reqInfo.Body.Length + EFBinaryRequestInfo.HEADERE_SIZE);

            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(BitConverter.GetBytes(totalSize));
            dataSource.AddRange(BitConverter.GetBytes((Int16)reqInfo.PacketID));
            dataSource.AddRange(new byte[1]);
            dataSource.AddRange(reqInfo.Body);

            session.Send(dataSource.ToArray(), 0, dataSource.Count);
        }
    }


    public class NetworkSession : AppSession<NetworkSession, EFBinaryRequestInfo>
    {
    }
}
