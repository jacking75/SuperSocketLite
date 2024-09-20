using System;

using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

using CommonLib;


namespace GateServer;

public class MainServer : AppServer<ClientSession, EFBinaryRequestInfo>
{
    public static ServerOption s_ServerOption;
    public static SuperSocket.SocketBase.Logging.ILog s_MainLogger;

    SuperSocket.SocketBase.Config.IServerConfig _config;

    PacketProcessor _mainPacketProcessor = new ();

    ClientSessionManager _clientSessionMgr = new ();
    
    
    public MainServer()
        : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
    {
        NewSessionConnected += new SessionHandler<ClientSession>(OnConnected);
        SessionClosed += new SessionHandler<ClientSession, CloseReason>(OnClosed);
        NewRequestReceived += new RequestHandler<ClientSession, EFBinaryRequestInfo>(OnPacketReceived);
    }

    public void InitConfig(ServerOption option)
    {
        s_ServerOption = option;

        _config = new SuperSocket.SocketBase.Config.ServerConfig
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
            bool bResult = Setup(new SuperSocket.SocketBase.Config.RootConfig(), _config, logFactory: new NLogLogFactory());

            if (bResult == false)
            {
                Console.WriteLine("[ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
                return;
            }
            else
            {
                s_MainLogger = base.Logger;
                s_MainLogger.Info("서버 초기화 성공");
            }


            CreateComponent();

            Start();

            s_MainLogger.Info("서버 생성 성공");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] 서버 생성 실패: {ex.ToString()}");
        }          
    }

    
    public void StopServer()
    {            
        Stop();

        _mainPacketProcessor.Destory();
    }

    public ErrorCode CreateComponent()
    {
        _clientSessionMgr.Init(_config.MaxConnectionNumber);
                    
        _mainPacketProcessor = new PacketProcessor();
        _mainPacketProcessor.CreateAndStart(SendData);

        s_MainLogger.Info("CreateComponent - Success");
        return ErrorCode.None;
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
            MainServer.s_MainLogger.Error($"{ex.ToString()},  {ex.StackTrace}");

            session.SendEndWhenSendingTimeOut(); 
            session.Close();
        }
    }

    public void Distribute(ServerPacketData requestPacket)
    {
        _mainPacketProcessor.InsertPacket(requestPacket);
    }
                    
    void OnConnected(ClientSession session)
    {
        //옵션의 최대 연결 수를 넘으면 SuperSocket이 바로 접속을 짤라버린다. 즉 이 OnConneted 함수가 호출되지 않는다

        if(_clientSessionMgr.NewSession(session) == false)
        {
            //TODO 최대 접속 수를 넘었으니 클라이언트에게 연결을 끊어라고 통보한다.

            s_MainLogger.Info(string.Format("세션 번호 {0} 접속", session.SessionID));
            return;
        }

        s_MainLogger.Info(string.Format("세션 번호 {0} 접속", session.SessionID));
                    
        //var packet = ServerPacketData.MakeNTFInConnectOrDisConnectClientPacket(true, session.SessionID, session.SessionIndex);            
        //Distribute(packet);
    }

    void OnClosed(ClientSession session, CloseReason reason)
    {
        s_MainLogger.Info(string.Format("세션 번호 {0} 접속해제: {1}", session.SessionID, reason.ToString()));

        //TODO:로그인을 성공한 경우라면 게임서버에 통보해야 한다.
        
        _clientSessionMgr.ColesdSession(session);
    }

    void OnPacketReceived(ClientSession session, EFBinaryRequestInfo reqInfo)
    {
        s_MainLogger.Debug(string.Format("세션 번호 {0} 받은 데이터 크기: {1}, ThreadId: {2}", session.SessionID, reqInfo.Body.Length, System.Threading.Thread.CurrentThread.ManagedThreadId));

        if(reqInfo.PacketID == (Int16)PacketId.ReqLogin)
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
        MainServer.s_MainLogger.Debug("로그인 요청 받음");

        try
        {
            //TODO 로그인 처리하기

            MainServer.s_MainLogger.Debug("로그인 요청 답변 보냄");

        }
        catch (Exception ex)
        {
            // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }
}


