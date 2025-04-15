﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;
using System.Diagnostics;

using SuperSocketLite.SocketBase.Logging;
using SuperSocketLite.SocketBase;
using SuperSocketLite.SocketBase.Protocol;
using SuperSocketLite.SocketBase.Config;

namespace MultiPortServer;

class MainServer : AppServer<NetworkSession, EFBinaryRequestInfo>
{
    public static SuperSocketLite.SocketBase.Logging.ILog MainLogger;

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

        MainLogger.Info("핸들러 등록 완료");
    }

    public void InitConfig(string name, int port, int maxConnectionNumber)
    {
        m_Config = new ServerConfig
        {
            Port = port,
            Ip = "Any",
            MaxConnectionNumber = maxConnectionNumber,
            Mode = SocketMode.Tcp,
            Name = name
        };
    }

    public void CreateServer()
    {
        try
        {
            bool bResult = Setup(new RootConfig(), m_Config, logFactory: new NLogLogFactory());

            if (bResult == false)
            {
                Console.WriteLine("[ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
                return;
            }
            else
            {
                MainLogger = base.Logger;
            }

            RegistHandler();

            MainLogger.Info("서버 생성 성공");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[ERROR] 서버 생성 실패: {ex.ToString()}");
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
        MainLogger.Info($"세션 번호 {session.SessionID} 접속");
    }

    void OnClosed(NetworkSession session, CloseReason reason)
    {
        MainLogger.Info($"세션 번호 {session.SessionID} 접속해제: {reason.ToString()}");
    }

    void RequestReceived(NetworkSession session, EFBinaryRequestInfo reqInfo)
    {
        MainLogger.Info($"세션 번호 {session.SessionID}, 받은 데이터 크기: {reqInfo.Body.Length}");
        
        var PacketID = reqInfo.PacketID;
        
        if (HandlerMap.ContainsKey(PacketID))
        {
            HandlerMap[PacketID](session, reqInfo);
        }
        else
        {
            MainLogger.Info($"세션 번호 {session.SessionID}, 받은 데이터 크기: {reqInfo.Body.Length}");
        }
    }
}


public class NetworkSession : AppSession<NetworkSession, EFBinaryRequestInfo>
{
}
