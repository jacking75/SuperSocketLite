﻿using System;
using System.Collections.Generic;

using SuperSocketLite.SocketBase.Logging;
using SuperSocketLite.SocketBase;
using SuperSocketLite.SocketBase.Protocol;
using SuperSocketLite.SocketBase.Config;
using System.Threading;

namespace GameServer_01;

/// <summary>
/// 메인 서버 클래스입니다.
/// </summary>
class MainServer : AppServer<NetworkSession, PacketRequestInfo>
{
    /// <summary>
    /// 메인 로거 인스턴스입니다.
    /// </summary>
    public static ILog s_MainLogger;

    private IServerConfig _config;
    private bool _isRun = false;
    private Thread _threadCount;

    /// <summary>
    /// MainServer 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public MainServer()
        : base(new DefaultReceiveFilterFactory<PacketReceiveFilter, PacketRequestInfo>())
    {
        NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
        SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
        NewRequestReceived += new RequestHandler<NetworkSession, PacketRequestInfo>(RequestReceived);
    }

    /// <summary>
    /// 핸들러를 등록합니다.
    /// </summary>
    private void RegistHandler()
    {
        // 에코 서버라서 핸들러 등록은 하지 않음
        
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
            bool isResult = Setup(new RootConfig(), _config, logFactory: new ZLoggerLogFactory());

            if (isResult == false)
            {
                Console.WriteLine("[ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
                return;
            }

            s_MainLogger = base.Logger;

            RegistHandler();

            _isRun = true;
            _threadCount = new Thread(EchoCounter);
            _threadCount.Start();

            s_MainLogger.Info($"[{DateTime.Now}] 서버 생성 성공");
        }
        catch (Exception ex)
        {
            s_MainLogger.Error($"서버 생성 실패: {ex.ToString()}");
        }
    }

    /// <summary>
    /// 서버를 종료합니다.
    /// </summary>
    public void Destory()
    {
        base.Stop();

        _isRun = false;
        _threadCount.Join();
    }

    private Int64 Count = 0;

    /// <summary>
    /// Echo 카운터를 실행합니다.
    /// </summary>
    private void EchoCounter()
    {
        while (_isRun)
        {
            Thread.Sleep(1000);

            var value = Interlocked.Exchange(ref Count, 0);
            //Console.WriteLine($"{DateTime.Now} : {value}");
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
    /// 클라이언트가 접속했을 때 호출됩니다.
    /// </summary>
    /// <param name="session">접속한 세션</param>
    private void OnConnected(NetworkSession session)
    {
        s_MainLogger.Debug($"[{DateTime.Now}] 세션 번호 {session.SessionID} 접속 start, ThreadId: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
    }

    /// <summary>
    /// 클라이언트가 접속을 해제했을 때 호출됩니다.
    /// </summary>
    /// <param name="session">해제된 세션</param>
    /// <param name="reason">해제 사유</param>
    private void OnClosed(NetworkSession session, CloseReason reason)
    {
        s_MainLogger.Info($"[{DateTime.Now}] 세션 번호 {session.SessionID},  접속해제: {reason.ToString()}");
    }

    /// <summary>
    /// 클라이언트로부터 요청을 받았을 때 호출됩니다.
    /// </summary>
    /// <param name="session">요청을 받은 세션</param>
    /// <param name="reqInfo">받은 요청 정보</param>
    private void RequestReceived(NetworkSession session, PacketRequestInfo reqInfo)
    {
        s_MainLogger.Debug($"[{DateTime.Now}] 세션 번호 {session.SessionID},  받은 데이터 크기: {reqInfo.Body.Length}, ThreadId: {System.Threading.Thread.CurrentThread.ManagedThreadId}");

        Interlocked.Increment(ref Count);


        var totalSize = (Int16)(reqInfo.Body.Length + PacketRequestInfo.HeaderSize);

        List<byte> dataSource =
        [
            .. BitConverter.GetBytes(totalSize),
            .. BitConverter.GetBytes((Int16)reqInfo.PacketID),
            .. new byte[1],
            .. reqInfo.Body,
        ];

        session.Send(dataSource.ToArray(), 0, dataSource.Count);
    }
}


/// <summary>
/// 네트워크 세션 클래스입니다.
/// </summary>
public class NetworkSession : AppSession<NetworkSession, PacketRequestInfo>
{
}
