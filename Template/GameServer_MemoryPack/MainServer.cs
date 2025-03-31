using System;
using System.Collections.Generic;

using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using System.Threading;

namespace GameServer_MemoryPack;

/// <summary>
/// 메인 서버 클래스입니다.
/// </summary>
class MainServer : AppServer<NetworkSession, PacketRequestInfo>
{
    public static ILog s_MainLogger;

    PacketProcessor _packetProcessor = new ();

    private IServerConfig _config;


    // MainServer 클래스의 새 인스턴스를 초기화합니다.
    public MainServer()
        : base(new DefaultReceiveFilterFactory<PacketReceiveFilter, PacketRequestInfo>())
    {
        NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
        SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
        NewRequestReceived += new RequestHandler<NetworkSession, PacketRequestInfo>(OnPacketReceived);
    }

    // 핸들러를 등록합니다.
    private void RegistHandler()
    {
        // 에코 서버라서 핸들러 등록은 하지 않음
        
        s_MainLogger.Info("핸들러 등록 완료");
    }

    // 서버 설정을 초기화합니다.
    public void InitConfig(ServerOption option)
    {
        _config = new ServerConfig
        {
            Port = option.Port,
            Ip = "Any",
            MaxConnectionNumber = option.MaxConnectionNumber,
            Mode = SocketMode.Tcp,
            Name = option.Name
        };
    }

    // 서버를 생성합니다.
    public void CreateServer(ServerOption serverOpt)
    {
        try
        {
            bool isResult = Setup(new RootConfig(), _config, logFactory: new ZLoggerLogFactory());

            if (isResult == false)
            {
                Console.WriteLine("[ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
                return;
            }


            s_MainLogger = base.Logger;
            CreateComponent(serverOpt);
                        
            s_MainLogger.Info($"[{DateTime.Now}] 서버 생성 성공");
        }
        catch (Exception ex)
        {
            s_MainLogger.Error($"서버 생성 실패: {ex.ToString()}");
        }
    }

    // 서버를 종료합니다.
    public void Destory()
    {
        base.Stop();
    }

    public ErrorCode CreateComponent(ServerOption serverOpt)
    {        
        _packetProcessor = new PacketProcessor();
        _packetProcessor.NetSendFunc = this.SendData;
        _packetProcessor.CreateAndStart(serverOpt);

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
        catch (Exception ex)
        {
            // TimeoutException 예외가 발생할 수 있다
            s_MainLogger.Error($"{ex.ToString()},  {ex.StackTrace}");

            session.SendEndWhenSendingTimeOut();
            session.Close();
        }
        return true;
    }

    public void Distribute(PacketRequestInfo requestPacket)
    {
        _packetProcessor.InsertPacket(requestPacket);
    }

    
    // 클라이언트가 접속했을 때 호출됩니다.
    private void OnConnected(NetworkSession session)
    {
        s_MainLogger.Debug($"[{DateTime.Now}] 세션 번호 {session.SessionID} 접속 start, ThreadId: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
    }

    // 클라이언트가 접속을 해제했을 때 호출됩니다.
    private void OnClosed(NetworkSession session, CloseReason reason)
    {
        s_MainLogger.Info($"[{DateTime.Now}] 세션 번호 {session.SessionID},  접속해제: {reason.ToString()}");
    }

    // 클라이언트로부터 요청을 받았을 때 호출됩니다.
    private void OnPacketReceived(NetworkSession session, PacketRequestInfo reqInfo)
    {
        s_MainLogger.Debug($"세션 번호 {session.SessionID} 받은 데이터 크기: {reqInfo.Body.Length}, ThreadId: {Thread.CurrentThread.ManagedThreadId}");

        reqInfo.SessionID = session.SessionID;
        Distribute(reqInfo);
    }
}


/// <summary>
/// 네트워크 세션 클래스입니다.
/// </summary>
public class NetworkSession : AppSession<NetworkSession, PacketRequestInfo>
{
}
