# Chapter 1: 애플리케이션 서버 (AppServer)

SuperSocketLite를 사용한 네트워크 애플리케이션 개발 여정에 오신 것을 환영합니다! 첫 번째 장에서는 SuperSocketLite 애플리케이션의 핵심 두뇌 역할을 하는 **애플리케이션 서버 (AppServer)**에 대해 알아보겠습니다.

AppServer는 마치 레스토랑의 총지배인과 같습니다. 지배인은 레스토랑 운영 전반을 관리하며, 손님(클라이언트)을 맞이하고, 주문을 받고(요청 처리), 주방(요청 처리 로직)에 전달하는 등 모든 과정을 조율합니다. 마찬가지로, AppServer는 클라이언트 연결을 관리하고, 들어오는 요청을 처리하며, 서버의 시작과 중지, 설정을 담당하는 등 서버 애플리케이션의 모든 부분을 조율하는 중심적인 역할을 합니다.

이 장을 통해 AppServer가 무엇인지, 왜 필요한지, 그리고 어떻게 사용하는지에 대한 기본적인 이해를 얻게 될 것입니다.

## AppServer는 왜 필요할까요?

간단한 "에코(echo)" 서버를 만든다고 상상해 봅시다. 이 서버는 클라이언트가 메시지를 보내면 받은 메시지 그대로 다시 클라이언트에게 돌려주는 역할을 합니다. 이런 서버를 만들려면 다음과 같은 기능들이 필요합니다.

1.  **네트워크 연결 수신 대기:** 특정 포트에서 클라이언트의 연결 요청을 기다려야 합니다.
2.  **연결 관리:** 여러 클라이언트가 동시에 접속할 수 있으므로, 각 연결을 식별하고 관리해야 합니다.
3.  **데이터 수신 및 처리:** 클라이언트로부터 데이터를 받고, 그 데이터가 완전한 메시지인지 확인해야 합니다. (예: "Hello" 메시지가 "He"와 "llo"로 나뉘어 도착할 수 있음)
4.  **요청 처리:** 완전한 메시지를 받으면, 정의된 로직(이 경우, 그대로 다시 보내기)을 실행해야 합니다.
5.  **데이터 전송:** 처리 결과를 클라이언트에게 다시 보내야 합니다.
6.  **연결 종료 처리:** 클라이언트가 연결을 끊으면 관련 리소스를 정리해야 합니다.

이 모든 과정을 직접 밑바닥부터 구현하는 것은 복잡하고 오류가 발생하기 쉽습니다. AppServer는 이러한 복잡한 네트워크 관련 작업들을 추상화하여 개발자가 애플리케이션의 핵심 로직(예: 에코 기능 구현)에 더 집중할 수 있도록 도와줍니다. AppServer는 연결 수락, 세션 관리, 데이터 수신/파싱, 요청 분배 등의 기본적인 틀을 제공합니다.

## AppServer 기본 사용법

SuperSocketLite에서 서버를 만들기 위한 첫걸음은 `AppServer` 클래스를 상속받는 것입니다.

```csharp
// 네임스페이스 추가
using SuperSocketLite.SocketBase;
using SuperSocketLite.SocketBase.Protocol;

// 간단한 세션 클래스 (다음 장에서 자세히 다룹니다)
public class MySession : AppSession<MySession, StringRequestInfo>
{
    // 세션 관련 로직 추가 가능
}

// AppServer를 상속받는 메인 서버 클래스
public class MyServer : AppServer<MySession, StringRequestInfo>
{
    public MyServer() : base(new DefaultReceiveFilterFactory<CommandLineReceiveFilter, StringRequestInfo>()) // 수신 필터 설정 (4장에서 자세히 다룸)
    {
        // 이벤트 핸들러 등록
        this.NewSessionConnected += MyServer_NewSessionConnected; // 새 클라이언트 연결 시
        this.SessionClosed += MyServer_SessionClosed;           // 클라이언트 연결 종료 시
        this.NewRequestReceived += MyServer_NewRequestReceived;   // 새 요청 수신 시
    }

    // 새 클라이언트가 연결되었을 때 호출될 메서드
    void MyServer_NewSessionConnected(MySession session)
    {
        Console.WriteLine($"클라이언트 {session.RemoteEndPoint} 연결됨!");
        session.Send("환영합니다!"); // 클라이언트에게 환영 메시지 전송
    }

    // 클라이언트 연결이 종료되었을 때 호출될 메서드
    void MyServer_SessionClosed(MySession session, CloseReason reason)
    {
        Console.WriteLine($"클라이언트 {session.RemoteEndPoint} 연결 종료됨 (이유: {reason})");
    }

    // 클라이언트로부터 새로운 요청을 받았을 때 호출될 메서드
    void MyServer_NewRequestReceived(MySession session, StringRequestInfo requestInfo)
    {
        Console.WriteLine($"클라이언트 {session.RemoteEndPoint}로부터 요청 받음: Key={requestInfo.Key}, Body={requestInfo.Body}");
        // 에코 기능: 받은 메시지 앞에 "Echo: "를 붙여서 돌려줌
        session.Send($"Echo: {requestInfo.Key} {requestInfo.Body}");
    }
}
```

위 코드에서 몇 가지 중요한 점을 살펴보겠습니다.

*   `AppServer<MySession, StringRequestInfo>`: `AppServer`를 상속받을 때는 두 가지 제네릭 타입을 지정합니다.
    *   `MySession`: 각 클라이언트 연결을 나타내는 클래스입니다. [제2장: 애플리케이션 세션 (AppSession)](02_애플리케이션_세션__appsession_.md)에서 자세히 다룹니다.
    *   `StringRequestInfo`: 클라이언트로부터 받은 요청 정보를 담는 클래스입니다. 여기서는 간단한 문자열 기반 요청을 가정합니다. [제3장: 요청 정보 (RequestInfo)](03_요청_정보__requestinfo_.md)에서 자세히 다룹니다.
*   `base(...)`: 부모 클래스 생성자를 호출하며, 여기서는 클라이언트로부터 데이터를 어떻게 파싱할지 정의하는 `ReceiveFilterFactory`를 전달합니다. [제4장: 수신 필터 (ReceiveFilter)](04_수신_필터__receivefilter_.md)에서 자세히 다룹니다.
*   `NewSessionConnected`, `SessionClosed`, `NewRequestReceived`: AppServer는 클라이언트 연결, 종료, 요청 수신과 같은 주요 이벤트가 발생했을 때 알려주는 이벤트들을 제공합니다. 우리는 이 이벤트들에 우리의 처리 메서드(예: `MyServer_NewSessionConnected`)를 등록하여 원하는 동작을 수행할 수 있습니다.

이제 이 서버를 설정하고 시작하는 코드를 보겠습니다.

```csharp
using SuperSocketLite.SocketBase.Config; // 설정 관련 네임스페이스

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("서버 시작 중...");

        var myServer = new MyServer();

        // 서버 설정 생성
        var serverConfig = new ServerConfig
        {
            Port = 2024, // 클라이언트가 접속할 포트 번호
            Ip = "Any",   // 모든 IP 주소에서 들어오는 연결을 허용
            MaxConnectionNumber = 100, // 최대 동시 접속자 수
            Name = "MyEchoServer" // 서버 이름
        };

        // 서버 설정 적용 및 시작
        if (!myServer.Setup(serverConfig))
        {
            Console.WriteLine("서버 설정 실패!");
            Console.ReadKey();
            return;
        }

        if (!myServer.Start())
        {
            Console.WriteLine("서버 시작 실패!");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("서버 시작 성공! 종료하려면 'q'를 누르세요.");

        // 'q'를 입력받을 때까지 서버 실행 유지
        while (Console.ReadKey().KeyChar != 'q')
        {
            Console.WriteLine();
            continue;
        }

        // 서버 중지
        myServer.Stop();
        Console.WriteLine("서버 종료됨.");
    }
}
```

*   `ServerConfig`: 서버의 동작 방식을 정의하는 설정 객체입니다. 포트 번호, IP 주소, 최대 연결 수 등을 지정할 수 있습니다.
*   `myServer.Setup(serverConfig)`: 생성된 설정 객체를 사용하여 서버를 초기화합니다. 이 단계에서 AppServer는 리스너 설정, 보안 설정 등을 준비합니다.
*   `myServer.Start()`: 설정을 바탕으로 실제 서버를 시작합니다. 이제 서버는 지정된 포트에서 클라이언트 연결을 기다리기 시작합니다.
*   `myServer.Stop()`: 실행 중인 서버를 중지하고 모든 활성 연결을 닫습니다.

이 코드를 실행하면, 2024번 포트에서 클라이언트 연결을 기다리는 에코 서버가 실행됩니다. 클라이언트가 연결하고 메시지를 보내면, 서버는 해당 메시지를 콘솔에 출력하고 "Echo: " 접두사를 붙여 클라이언트에게 다시 보냅니다.

## AppServer 내부 동작 방식 (간략히)

AppServer가 어떻게 클라이언트 연결을 받고 요청을 처리하는지 내부 흐름을 간단히 살펴보겠습니다. (모든 세부 사항을 다루지는 않습니다.)

1.  **서버 시작 (`Start`)**: `AppServer`는 내부적으로 [제5장: 소켓 서버 (SocketServer)](05_소켓_서버__socketserver_.md)를 사용하여 실제 네트워크 리스닝을 시작합니다. [제7장: 소켓 리스너 (SocketListener)](07_소켓_리스너__socketlistener_.md)가 지정된 IP와 포트에서 클라이언트 연결을 기다립니다.
2.  **클라이언트 연결**: 클라이언트가 서버에 연결을 시도하면, `SocketListener`가 이를 감지하고 연결을 수락합니다.
3.  **세션 생성**: `SocketServer`는 수락된 연결을 바탕으로 [제6장: 소켓 세션 (SocketSession)](06_소켓_세션__socketsession_.md) 객체를 만듭니다. 이 객체는 실제 소켓 통신을 담당합니다. 그리고 `AppServer`에게 새 연결이 생겼음을 알립니다.
4.  **AppSession 생성 및 등록**: `AppServer`는 `SocketSession` 정보를 받아 우리가 정의한 `MySession` 같은 [제2장: 애플리케이션 세션 (AppSession)](02_애플리케이션_세션__appsession_.md) 객체를 생성합니다. 이 `AppSession` 객체는 클라이언트와의 논리적인 연결 상태와 데이터를 관리합니다. 생성된 `AppSession`은 AppServer 내부의 세션 목록(예: `m_SessionDict`)에 등록됩니다.
5.  **`NewSessionConnected` 이벤트 발생**: `AppServer`는 `NewSessionConnected` 이벤트를 발생시켜 우리가 등록한 `MyServer_NewSessionConnected` 메서드를 호출합니다.
6.  **데이터 수신**: 클라이언트가 데이터를 보내면 `SocketSession`이 데이터를 수신합니다.
7.  **데이터 파싱 ([수신 필터 (ReceiveFilter)](04_수신_필터__receivefilter_.md))**: 수신된 데이터는 `AppServer` 설정 시 지정된 [제4장: 수신 필터 (ReceiveFilter)](04_수신_필터__receivefilter_.md)로 전달됩니다. `ReceiveFilter`는 네트워크 스트림에서 의미 있는 단위의 요청(예: 한 줄의 텍스트, 특정 구분자로 끝나는 데이터)을 추출하고, 이를 [제3장: 요청 정보 (RequestInfo)](03_요청_정보__requestinfo_.md) 객체(예: `StringRequestInfo`)로 변환합니다.
8.  **`NewRequestReceived` 이벤트 발생**: `ReceiveFilter`가 완전한 요청을 만들어내면, `AppServer`는 `NewRequestReceived` 이벤트를 발생시켜 우리가 등록한 `MyServer_NewRequestReceived` 메서드를 호출합니다. 이 때 파싱된 `RequestInfo` 객체가 함께 전달됩니다.
9.  **데이터 전송 (`Send`)**: `AppSession`의 `Send` 메서드를 호출하면, 데이터는 내부적으로 `SocketSession`을 통해 클라이언트에게 전송됩니다.
10. **연결 종료**: 클라이언트가 연결을 끊거나 서버에서 연결을 닫으면, 관련 `AppSession`이 세션 목록에서 제거되고 `SessionClosed` 이벤트가 발생합니다.

이 과정을 간단한 다이어그램으로 표현하면 다음과 같습니다.

```mermaid
sequenceDiagram
    participant C as 클라이언트
    participant SL as 소켓 리스너
    participant SS as 소켓 서버
    participant AS as AppServer
    participant APPS as AppSession
    participant RF as 수신 필터

    C->>+SL: 연결 요청
    SL->>+SS: 연결 수락
    SS->>+AS: 새 소켓 세션 알림
    AS->>+APPS: AppSession 생성 및 등록
    AS-->>-SS: 등록 완료
    AS-->>-SL: 처리 완료
    Note over AS: NewSessionConnected 이벤트 발생
    C->>APPS: 데이터 전송
    APPS->>+RF: 데이터 전달 (파싱 요청)
    RF-->>-APPS: RequestInfo 반환
    Note over AS: NewRequestReceived 이벤트 발생 (RequestInfo와 함께)
    APPS->>C: 응답 데이터 전송
    C->>APPS: 연결 종료 요청
    Note over AS: SessionClosed 이벤트 발생
```

실제 코드를 살펴보면, 이러한 흐름을 더 명확하게 이해할 수 있습니다.

**`AppServerBase.Start()` (SuperSocketLite\\SocketBase\\AppServerBase.cs)**

```csharp
// AppServerBase 클래스 내 Start 메서드
public virtual bool Start()
{
    // ... 상태 확인 및 초기화 ...

    // 내부 SocketServer 시작
    if (!m_SocketServer.Start())
    {
        m_StateCode = ServerStateConst.NotStarted;
        return false;
    }

    // ... 타이머 시작 등 추가 설정 ...

    // 시작 시간 기록 및 상태 변경
    StartedTime = DateTime.Now;
    m_StateCode = ServerStateConst.Running;

    // ... OnStarted 이벤트 호출 ...

    return true;
}
```

`AppServer`의 `Start` 메서드는 내부적으로 `AppServerBase`의 `Start`를 호출하고, 이는 다시 `m_SocketServer` (실제 소켓 작업을 담당하는 객체)의 `Start`를 호출하여 리스닝을 시작합니다.

**`AppServer.RegisterSession()` (SuperSocketLite\\SocketBase\\AppServer.cs)**

```csharp
// AppServer<TAppSession, TRequestInfo> 클래스 내 RegisterSession 메서드
protected override bool RegisterSession(string sessionID, TAppSession appSession)
{
    // 내부 세션 딕셔너리에 세션 추가 시도
    if (m_SessionDict.TryAdd(sessionID, appSession))
        return true; // 성공

    // 이미 동일한 ID의 세션이 존재하면 실패 처리 및 로깅
    if (Logger.IsErrorEnabled)
    {
        var message = "세션 ID가 이미 존재하여 세션 등록을 거부합니다!";
        Logger.Error(string.Format("Session: {0}/{1}", appSession.SessionID, appSession.RemoteEndPoint) + Environment.NewLine + message);
    }
    
    return false; // 실패
}
```

새 클라이언트 연결이 수립되면 `AppServer`는 이 `RegisterSession` 메서드를 호출하여 내부 `ConcurrentDictionary`인 `m_SessionDict`에 새로 생성된 `AppSession`을 저장합니다. 이를 통해 `GetSessionByID` 같은 메서드로 특정 세션을 찾을 수 있습니다.

**`AppServerBase.OnNewSessionConnected()` (SuperSocketLite\\SocketBase\\AppServerBase.cs)**

```csharp
// AppServerBase 클래스 내 OnNewSessionConnected 메서드
protected virtual void OnNewSessionConnected(TAppSession session)
{
    var handler = m_NewSessionConnected; // 등록된 이벤트 핸들러 가져오기
    if (handler == null)
    {
        return; // 핸들러가 없으면 아무것도 안 함
    }

    // 별도의 스레드에서 핸들러 실행 (비동기 처리)
    Task.Run(() => handler(session));            
}
```

세션이 성공적으로 등록되면 `AppServerBase`는 이 `OnNewSessionConnected` 메서드를 호출하고, 이 메서드는 사용자가 등록한 `NewSessionConnected` 이벤트 핸들러(예: `MyServer_NewSessionConnected`)를 비동기적으로 실행합니다. `SessionClosed`와 `NewRequestReceived` 이벤트 처리도 비슷한 방식으로 이루어집니다.

이처럼 AppServer는 SuperSocketLite의 다른 핵심 구성 요소들([AppSession](02_애플리케이션_세션__appsession_.md), [RequestInfo](03_요청_정보__requestinfo_.md), [ReceiveFilter](04_수신_필터__receivefilter_.md), [SocketServer](05_소켓_서버__socketserver_.md), [SocketSession](06_소켓_세션__socketsession_.md), [SocketListener](07_소켓_리스너__socketlistener_.md))과 상호작용하며 서버의 전체적인 생명 주기와 요청 처리 흐름을 관리합니다.

## 결론

이번 장에서는 SuperSocketLite 애플리케이션의 심장과 같은 **AppServer**에 대해 알아보았습니다. AppServer는 서버 설정, 시작/중지, 클라이언트 연결 관리, 요청 수신 및 처리를 위한 중심적인 역할을 수행하며, 복잡한 네트워크 프로그래밍을 추상화하여 개발자가 애플리케이션 로직에 집중할 수 있도록 돕는다는 것을 배웠습니다. 간단한 에코 서버 예제를 통해 AppServer를 상속받고, 설정하고, 시작하는 기본적인 방법을 살펴보았습니다.

AppServer가 클라이언트 연결을 관리한다고 했는데, 이 '연결'은 SuperSocketLite에서 어떻게 표현될까요? 다음 장에서는 개별 클라이언트 연결을 나타내는 **[제2장: 애플리케이션 세션 (AppSession)](02_애플리케이션_세션__appsession_.md)**에 대해 자세히 알아보겠습니다. AppSession은 각 클라이언트의 상태를 유지하고 데이터를 주고받는 통로 역할을 합니다.

---

Generated by [AI Codebase Knowledge Builder](https://github.com/The-Pocket/Tutorial-Codebase-Knowledge)