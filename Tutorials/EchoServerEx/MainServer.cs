using System;
using System.Collections.Generic;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;

namespace EchoServerEx;


/// <summary>
/// 메인 서버 클래스입니다.
/// </summary>
class MainServer : AppServer<NetworkSession, EFBinaryRequestInfo>
{
    /// <summary>
    /// 메인 로거 인스턴스입니다.
    /// </summary>
    public static SuperSocket.SocketBase.Logging.ILog s_MainLogger;

    /// <summary>
    /// 패킷 핸들러 맵입니다.
    /// </summary>
    Dictionary<int, Action<NetworkSession, EFBinaryRequestInfo>> HandlerMap = new Dictionary<int, Action<NetworkSession, EFBinaryRequestInfo>>();

    /// <summary>
    /// 공통 핸들러 인스턴스입니다.
    /// </summary>
    CommonHandler _commonHandler = new CommonHandler();

    /// <summary>
    /// 서버 설정 인스턴스입니다.
    /// </summary>
    IServerConfig _config;

    /// <summary>
    /// MainServer 클래스의 생성자입니다.
    /// </summary>
    public MainServer()
        : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
    {
        NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
        SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
        NewRequestReceived += new RequestHandler<NetworkSession, EFBinaryRequestInfo>(RequestReceived);
    }

    /// <summary>
    /// 핸들러를 등록합니다.
    /// </summary>
    void RegistHandler()
    {
        HandlerMap.Add((int)PacketId.ReqEcho, _commonHandler.RequestEcho);

        s_MainLogger.Info("핸들러 등록 완료");
    }

    /// <summary>
    /// 서버 설정을 초기화합니다.
    /// </summary>
    /// <param name="option">서버 옵션</param>
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

    /// <summary>
    /// 서버를 생성합니다.
    /// </summary>
    public void CreateServer()
    {
        try
        {
            bool isResult = Setup(new RootConfig(), _config, logFactory: new NLogLogFactory());

            if (isResult == false)
            {
                Console.WriteLine("[ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
                return;
            }
            else
            {
                s_MainLogger = base.Logger;
            }

            RegistHandler();

            s_MainLogger.Info("서버 생성 성공");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] 서버 생성 실패: {ex.ToString()}");
        }
    }

    /// <summary>
    /// 서버가 실행 중인지 확인합니다.
    /// </summary>
    /// <param name="curState">현재 서버 상태</param>
    /// <returns>서버가 실행 중인지 여부</returns>
    public bool IsRunning(ServerState curState)
    {
        if (curState == ServerState.Running)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 클라이언트가 접속했을 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    /// <param name="session">접속한 클라이언트 세션</param>
    void OnConnected(NetworkSession session)
    {
        s_MainLogger.Info($"세션 번호 {session.SessionID} 접속");
    }

    /// <summary>
    /// 클라이언트가 접속을 해제했을 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    /// <param name="session">해제된 클라이언트 세션</param>
    /// <param name="reason">접속 해제 사유</param>
    void OnClosed(NetworkSession session, CloseReason reason)
    {
        s_MainLogger.Info($"세션 번호 {session.SessionID} 접속해제: {reason.ToString()}");
    }

    /// <summary>
    /// 클라이언트로부터 요청을 받았을 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    /// <param name="session">요청을 받은 클라이언트 세션</param>
    /// <param name="reqInfo">받은 요청 정보</param>
    void RequestReceived(NetworkSession session, EFBinaryRequestInfo reqInfo)
    {
        var packetId = reqInfo.PacketID;

        if (HandlerMap.ContainsKey(packetId))
        {
            HandlerMap[packetId](session, reqInfo);
        }
        else
        {
            s_MainLogger.Info($"세션 번호 {session.SessionID}, 받은 데이터 크기: {reqInfo.Body.Length}");
        }
    }
}


/// <summary>
/// 네트워크 세션 클래스입니다.
/// </summary>
public class NetworkSession : AppSession<NetworkSession, EFBinaryRequestInfo>
{
}
