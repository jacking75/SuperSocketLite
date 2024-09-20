using System;
using System.Collections.Generic;

using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

using CSBaseLib;
using DB;


//TODO 1. 주기적으로 접속한 세션이 패킷을 주고 받았는지 조사(좀비 클라이언트 검사)

namespace ChatServer;

public class MainServer : AppServer<ClientSession, EFBinaryRequestInfo>
{
    public static ChatServerOption s_ServerOption;
    public static SuperSocket.SocketBase.Logging.ILog s_MainLogger;

    SuperSocket.SocketBase.Config.IServerConfig _config;        
    static RemoteConnectCheck RemoteCheck = null;
    PacketDistributor Distributor = new PacketDistributor();

    
    public MainServer()
        : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
    {
        NewSessionConnected += new SessionHandler<ClientSession>(OnConnected);
        SessionClosed += new SessionHandler<ClientSession, CloseReason>(OnClosed);
        NewRequestReceived += new RequestHandler<ClientSession, EFBinaryRequestInfo>(OnPacketReceived);
    }

    public void InitConfig(ChatServerOption option)
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

            if( CreateComponent() != ErrorCode.None)
            {
                return;
            }

            Start();

            StartRemoteConnect();
            
            s_MainLogger.Info("서버 생성 성공");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] 서버 생성 실패: {ex.ToString()}");
        }         
    }

    public void StartRemoteConnect()
    {
        RemoteCheck = new RemoteConnectCheck();

        var remoteInfoList = new List<Tuple<string, string, int>>();

        foreach(var server in ConfigTemp.RemoteServers)
        {
            var infoList = server.Split(":");
            remoteInfoList.Add(new Tuple<string, string, int>(infoList[0], infoList[1], infoList[2].ToInt32()));

            s_MainLogger.Info(string.Format("(To)연결할 서버 정보: {0}, {1}, {2}", infoList[0], infoList[1], infoList[2]));
        }

        RemoteCheck.Init(this, remoteInfoList);
    }

    public void StopServer()
    {            
        RemoteCheck.Stop();

        Stop();

        Distributor.Destory();
    }

    public ErrorCode CreateComponent()
    {
        ClientSession.CreateIndexPool(_config.MaxConnectionNumber);

        var error = Distributor.Create(this);

        if (error != ErrorCode.None)
        {
            s_MainLogger.Info($"CreateComponent - Failes. {error}");
            return error;
        }

        s_MainLogger.Info("CreateComponent - Success");
        return ErrorCode.None;
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
        catch(Exception)
        {
            session.SendEndWhenSendingTimeOut(); 
            session.Close();
        }
        return true;
    }

    public PacketDistributor GetPacketDistributor() { return Distributor; }
            
    void OnConnected(ClientSession session)
    {
        //옵션의 최대 연결 수를 넘으면 SuperSocket이 바로 접속을 짤라버린다. 즉 이 OnConneted 함수가 호출되지 않는다

        session.AllocSessionIndex();
        s_MainLogger.Info(string.Format("세션 번호 {0} 접속", session.SessionID));
                    
        var packet = ServerPacketData.MakeNTFInConnectOrDisConnectClientPacket(true, session.SessionID, session.SessionIndex);            
        Distributor.DistributeCommon(false, packet);
    }

    void OnClosed(ClientSession session, CloseReason reason)
    {
        s_MainLogger.Info(string.Format("세션 번호 {0} 접속해제: {1}", session.SessionID, reason.ToString()));


        var packet = ServerPacketData.MakeNTFInConnectOrDisConnectClientPacket(false, session.SessionID, session.SessionIndex);
        Distributor.DistributeCommon(false, packet);

        session.FreeSessionIndex(session.SessionIndex);
    }

    void OnPacketReceived(ClientSession session, EFBinaryRequestInfo reqInfo)
    {
        s_MainLogger.Debug(string.Format("세션 번호 {0} 받은 데이터 크기: {1}, ThreadId: {2}", session.SessionID, reqInfo.Body.Length, System.Threading.Thread.CurrentThread.ManagedThreadId));

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
