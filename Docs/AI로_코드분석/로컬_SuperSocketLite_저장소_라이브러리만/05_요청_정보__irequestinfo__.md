# Chapter 5: 요청 정보 (IRequestInfo)

[이전 챕터 (수신 필터 (IReceiveFilter))](04_수신_필터__ireceivefilter__.md)에서는 클라이언트가 보낸 알 수 없는 바이트 덩어리를 서버가 이해할 수 있는 메시지 단위로 번역해주는 '번역가', 즉 `IReceiveFilter`에 대해 배웠습니다. `IReceiveFilter`는 미리 약속된 규칙(프로토콜)에 따라 데이터를 분석하고, 완전한 요청 하나를 찾아내는 중요한 역할을 했죠.

하지만 번역가가 번역만 하면 끝일까요? 아닙니다! 번역된 내용은 주방(서버 로직)에서 실제로 사용될 수 있도록 깔끔하게 정리되어야 합니다. 예를 들어, 손님의 주문서를 번역했다면, 어떤 메뉴를 주문했는지(예: "파스타"), 추가 요청사항은 무엇인지(예: "치즈 많이") 명확하게 구분해서 주방에 전달해야 합니다.

바로 이 '잘 정리된 주문서' 역할을 하는 것이 **요청 정보 (IRequestInfo)** 입니다. `IReceiveFilter`가 열심히 번역하고 분석한 결과물이 바로 이 `IRequestInfo` 객체이며, 우리의 서버 애플리케이션 코드가 실제로 다루게 되는 데이터 구조입니다.

## 요청 정보(IRequestInfo)는 왜 필요할까요? (잘 정리된 주문서)

[이전 챕터](04_수신_필터__ireceivefilter__.md)에서 살펴본 채팅 서버 예제를 다시 생각해 봅시다. 클라이언트가 닉네임을 변경하기 위해 `/nick 멋진나\r\n` 라는 메시지를 보냈습니다. `TerminatorReceiveFilter`와 같은 [수신 필터 (IReceiveFilter)](04_수신_필터__ireceivefilter__.md)는 `\r\n`을 보고 `/nick 멋진나` 부분을 성공적으로 추출했습니다.

이제 이 `/nick 멋진나`라는 데이터를 어떻게 사용할까요? 이 문자열 안에는 어떤 종류의 요청인지 나타내는 **명령어("nick")** 와 명령어에 필요한 **데이터("멋진나")** 가 함께 들어 있습니다. 서버 로직(예: [AppServer](02_애플리케이션_서버__appserver___appserverbase__.md)의 `NewRequestReceived` 이벤트 핸들러)에서는 이 둘을 쉽게 구분해서 사용하고 싶을 것입니다.

만약 `IReceiveFilter`가 단순히 `/nick 멋진나`라는 문자열만 덜렁 넘겨준다면, `NewRequestReceived` 핸들러는 매번 이 문자열을 직접 파싱(분석)해서 명령어와 데이터를 분리하는 코드를 작성해야 할 것입니다. 모든 요청 처리 로직마다 이런 코드가 반복된다면 매우 비효율적이겠죠?

`IRequestInfo`는 바로 이 문제를 해결합니다. `IReceiveFilter`는 데이터를 분석한 후, 결과를 `IRequestInfo`라는 **구조화된 객체**에 담아 전달합니다. 이 객체 안에는 요청을 식별하는 **키(Key)** (예: 명령어 "NICK")와 요청 본문 **데이터(Body)** (예: "멋진나"), 그리고 필요하다면 더 자세한 **파라미터(Parameters)** (예: ["멋진나"]) 등이 이미 잘 정리되어 들어 있습니다.

덕분에 `NewRequestReceived` 핸들러에서는 복잡한 문자열 파싱 없이, 단순히 `requestInfo.Key`, `requestInfo.Body`와 같은 속성에 접근하여 필요한 정보를 바로 사용할 수 있습니다. 마치 주방에서 잘 정리된 주문서 한 장을 보고 바로 요리를 시작할 수 있는 것과 같습니다!

## `IRequestInfo`란 무엇인가요?

`IRequestInfo`는 SuperSocketLite에서 성공적으로 파싱된 **단일 클라이언트 요청**을 나타내는 **인터페이스(Interface)** 입니다. 인터페이스는 "이런 기능을 가져야 한다"는 약속과 같다고 했죠? `IRequestInfo` 인터페이스는 모든 요청 정보 객체가 최소한 다음과 같은 정보를 가져야 한다고 약속합니다.

*   **`Key` (string 타입)**: 이 요청이 어떤 종류의 요청인지를 식별하는 고유한 키 값입니다. 보통 명령어 이름(예: "LOGIN", "MSG", "NICK")이 여기에 해당합니다. `AppServer`는 이 `Key` 값을 보고 어떤 작업을 수행해야 할지 결정할 수 있습니다.

이 기본 `IRequestInfo` 인터페이스 외에도, 더 많은 정보를 담을 수 있는 확장된 인터페이스와 이를 구현하는 여러 기본 클래스들이 제공됩니다.

*   **`IRequestInfo<TRequestBody>`**: `Key` 외에 요청의 본문 데이터(`Body`)를 가지는 인터페이스입니다. `TRequestBody`는 본문 데이터의 타입(예: `string`, `byte[]`)을 나타냅니다.
*   **`RequestInfo<TRequestBody>`**: `IRequestInfo<TRequestBody>` 인터페이스를 구현한 기본 **클래스(Class)** 입니다. `Key`와 `Body` 속성을 가집니다.
*   **`StringRequestInfo` ([SocketBase/Protocol/StringRequestInfo.cs](SocketBase/Protocol/StringRequestInfo.cs))**: 가장 흔하게 사용되는 요청 정보 클래스 중 하나입니다.
    *   `RequestInfo<string>`을 상속받아, `Key` (string), `Body` (string)를 가집니다.
    *   추가로, `Body`를 공백과 같은 구분자로 분리한 **`Parameters` (string[] 타입)** 속성을 제공합니다. 예를 들어 `Body`가 "user1 password123" 이라면, `Parameters`는 `["user1", "password123"]` 배열이 될 수 있습니다.
*   **`BinaryRequestInfo` ([SocketBase/Protocol/BinaryRequestInfo.cs](SocketBase/Protocol/BinaryRequestInfo.cs))**: 요청 본문이 바이너리 데이터(예: 파일, 이미지)일 때 사용됩니다.
    *   `RequestInfo<byte[]>`를 상속받아, `Key` (string), `Body` (byte[] 타입)를 가집니다.
*   **기타**: HTTP 요청을 위한 `HttpRequestInfoBase` ([Protocol/HttpRequestInfoBase.cs](Protocol/HttpRequestInfoBase.cs)), UDP 요청을 위한 `UdpRequestInfo` ([SocketBase/Protocol/UdpRequestInfo.cs](SocketBase/Protocol/UdpRequestInfo.cs)) 등 특정 프로토콜에 특화된 요청 정보 클래스들도 있습니다.

어떤 `IReceiveFilter`를 사용하느냐에 따라, `NewRequestReceived` 이벤트로 전달되는 `requestInfo` 객체의 실제 타입이 달라집니다. 예를 들어, `TerminatorReceiveFilter`는 보통 `StringRequestInfo` 객체를 생성합니다.

## `IRequestInfo` 사용하기 (채팅 예제 완성)

이제 `IRequestInfo` (특히 `StringRequestInfo`)를 사용하여 [이전 챕터](04_수신_필터__ireceivefilter__.md)의 채팅 예제 코드를 더 명확하게 만들어 봅시다. `AppServer`의 `NewRequestReceived` 이벤트 핸들러 부분을 다시 살펴보겠습니다.

```csharp
// Chapter 3, 4 에서 이어지는 코드
using SuperSocketLite.SocketBase;
using SuperSocketLite.SocketBase.Protocol; // StringRequestInfo 사용

// ... (ChatSession 클래스 및 AppServer 설정 부분 생략) ...

// 클라이언트로부터 요청이 왔을 때 처리 (AppServer 이벤트)
// IReceiveFilter가 StringRequestInfo 객체를 성공적으로 만들어 전달했다고 가정합니다.
appServer.NewRequestReceived += (session, requestInfo) =>
{
    // session은 ChatSession 타입, requestInfo는 StringRequestInfo 타입입니다.

    // 1. requestInfo.Key를 사용하여 어떤 명령인지 확인합니다.
    Console.WriteLine($"[{session.SessionID}] 요청 수신: Key='{requestInfo.Key}', Body='{requestInfo.Body}'");

    // 2. Key가 "NICK" 인지 비교합니다. (대소문자 구분 없이)
    if (requestInfo.Key.Equals("NICK", StringComparison.OrdinalIgnoreCase))
    {
        // 3. requestInfo.Body에서 닉네임 부분을 가져옵니다. (양쪽 공백 제거)
        var newNickname = requestInfo.Body.Trim();
        if (!string.IsNullOrEmpty(newNickname))
        {
            session.Nickname = newNickname; // ChatSession에 닉네임 저장
            session.Send($"닉네임이 '{session.Nickname}'(으)로 설정되었습니다.\r\n");
            Console.WriteLine($"[{session.SessionID}] 닉네임 설정: {session.Nickname}");
        }
        else
        {
            session.Send("닉네임은 공백일 수 없습니다.\r\n");
        }
    }
    // 4. Key가 "MSG" 인지 비교합니다.
    else if (requestInfo.Key.Equals("MSG", StringComparison.OrdinalIgnoreCase))
    {
        if(string.IsNullOrEmpty(session.Nickname))
        {
             session.Send("닉네임을 먼저 설정해야 메시지를 보낼 수 있습니다. (/nick 이름)\r\n");
             return;
        }
        // 5. requestInfo.Body에 메시지 내용이 들어있습니다.
        var message = requestInfo.Body;
        Console.WriteLine($"[{session.Nickname}]: {message}"); // 콘솔에 출력

        // 실제 채팅 서버라면 여기서 다른 사용자들에게 메시지를 브로드캐스트 해야 합니다.
        // 예시: appServer.Broadcast($"[{session.Nickname}]: {message}\r\n");

        // 임시로 보낸 사람에게만 다시 보냄 (에코)
        session.Send($"[{session.Nickname}]: {message}\r\n");
    }
    // 6. 만약 Parameters 배열을 사용해야 한다면? (예: /BAN user1 1h "사유")
    else if (requestInfo.Key.Equals("BAN", StringComparison.OrdinalIgnoreCase))
    {
        // StringRequestInfo는 Parameters 속성을 제공합니다.
        if (requestInfo.Parameters != null && requestInfo.Parameters.Length >= 3)
        {
            string userToBan = requestInfo.Parameters[0]; // 첫 번째 파라미터
            string duration = requestInfo.Parameters[1];  // 두 번째 파라미터
            string reason = requestInfo.Parameters[2];   // 세 번째 파라미터
            Console.WriteLine($"차단 명령: 사용자={userToBan}, 기간={duration}, 사유={reason}");
            // ... 실제 차단 로직 구현 ...
            session.Send($"사용자 '{userToBan}' 차단 처리됨.\r\n");
        }
        else
        {
            session.Send("잘못된 BAN 명령 형식입니다. 예: /ban 사용자 기간 \"사유\"\r\n");
        }
    }
    else
    {
        // 7. AppServer가 처리 방법을 모르는 Key라면, ChatSession의 HandleUnknownRequest 호출
        session.InternalHandleUnknownRequest(requestInfo);
    }
};

// ... (서버 시작 및 종료 부분 생략) ...
```

위 코드를 통해 `IRequestInfo` (여기서는 `StringRequestInfo`)의 장점을 명확히 알 수 있습니다.

*   **복잡한 파싱 불필요:** `/nick 멋진나` 나 `/msg 안녕하세요` 같은 문자열을 직접 분석할 필요 없이, `requestInfo.Key`로 명령어를, `requestInfo.Body`로 전체 내용을, `requestInfo.Parameters`로 개별 파라미터를 바로 얻을 수 있습니다.
*   **코드 가독성 향상:** 요청 처리 로직이 훨씬 깔끔하고 이해하기 쉬워집니다. 어떤 명령에 대한 처리인지 `if (requestInfo.Key == ...)` 구문을 통해 명확히 알 수 있습니다.
*   **로직 분리 명확화:** 데이터 파싱(분석)의 책임은 [수신 필터 (IReceiveFilter)](04_수신_필터__ireceivefilter__.md)에게 맡기고, `NewRequestReceived` 핸들러는 잘 정리된 `IRequestInfo`를 사용하여 실제 **비즈니스 로직**(닉네임 변경, 메시지 전송 등)에만 집중할 수 있습니다.

만약 여러분이 정의한 프로토콜이 단순한 문자열 명령이 아니라 복잡한 구조의 바이너리 데이터라면, 해당 프로토콜에 맞는 커스텀 `IReceiveFilter`와 커스텀 `IRequestInfo` 클래스를 만들어 사용할 수도 있습니다. 예를 들어, 요청 본문이 JSON 문자열이라면, `RequestInfo<MyJsonObject>` 같은 형태를 사용할 수도 있습니다.

## `IRequestInfo`의 주요 속성 살펴보기

가장 일반적으로 사용되는 `StringRequestInfo`를 기준으로 주요 속성들을 다시 한번 정리해 보겠습니다.

*   **`Key` (string):** 요청의 종류를 나타내는 식별자입니다. 일반적으로 명령어(Command) 이름입니다. (예: "NICK", "MSG", "LOGIN")
*   **`Body` (string):** 요청의 본문 내용 전체입니다. `Key` 부분을 제외한 나머지 문자열 전체입니다. (예: "멋진나", "안녕하세요 반갑습니다")
*   **`Parameters` (string[]):** `Body` 문자열을 특정 구분자(기본적으로 공백)로 나눈 결과 배열입니다. 명령어에 여러 개의 인자(파라미터)가 필요한 경우 유용합니다. (예: "user1 password123" 이 `Body`일 때, `Parameters`는 `["user1", "password123"]` 입니다.)

**참고:** `StringRequestInfo`를 생성할 때 어떤 구분자를 사용하여 `Body`를 `Parameters`로 나눌지는 [수신 필터 (IReceiveFilter)](04_수신_필터__ireceivefilter__.md) 내부의 `BasicRequestInfoParser` ([SocketBase/Protocol/BasicRequestInfoParser.cs](SocketBase/Protocol/BasicRequestInfoParser.cs))와 같은 파서를 어떻게 설정하느냐에 따라 달라질 수 있습니다. 기본적으로는 공백(`" "`)을 사용합니다.

다른 종류의 `IRequestInfo` 구현체들은 각자의 필요에 맞는 속성들을 가집니다. 예를 들어 `BinaryRequestInfo`는 `Body` 속성의 타입이 `byte[]` 입니다.

## 내부 동작 방식 (간단히 엿보기)

`IRequestInfo` 객체는 어떻게 생성되어 우리 코드까지 전달될까요? 이 과정은 [수신 필터 (IReceiveFilter)](04_수신_필터__ireceivefilter__.md) 챕터에서 이미 살펴본 내용과 밀접하게 연관됩니다.

1.  [수신 필터 (IReceiveFilter)](04_수신_필터__ireceivefilter__.md)가 클라이언트로부터 받은 바이트 데이터를 분석하여 완전한 요청 하나를 식별합니다.
2.  식별된 요청 데이터를 바탕으로, 해당 필터에 설정된 `IRequestInfo` 타입의 객체를 생성합니다.
    *   예를 들어, `TerminatorReceiveFilter`는 보통 내부에 `BasicRequestInfoParser`를 사용하여 추출된 문자열 데이터(예: `/nick 멋진나`)를 `StringRequestInfo` 객체로 변환합니다. 이 과정에서 문자열을 파싱하여 `Key`("NICK"), `Body`("멋진나"), `Parameters`(["멋진나"])를 채웁니다.
    *   만약 `FixedHeaderReceiveFilter`를 사용하고 헤더와 바이너리 본문을 처리했다면, `BinaryRequestInfo` 또는 사용자가 정의한 커스텀 `RequestInfo` 객체를 생성할 수 있습니다. 이때 `Key` 값은 헤더 정보에서 추출하거나 미리 약속된 값을 사용할 수 있고, `Body`에는 본문 바이트 데이터가 담깁니다.
3.  생성된 `IRequestInfo` 객체는 `IReceiveFilter`의 `Filter` 메서드의 반환값으로 [애플리케이션 세션 (AppSession)](03_애플리케이션_세션__appsession__.md)에게 전달됩니다.
4.  `AppSession`은 이 `IRequestInfo` 객체를 [애플리케이션 서버 (AppServer / AppServerBase)](02_애플리케이션_서버__appserver___appserverbase__.md)에게 전달합니다.
5.  `AppServer`는 `NewRequestReceived` 이벤트를 발생시키면서, 해당 `AppSession` 객체와 함께 방금 생성된 `IRequestInfo` 객체를 이벤트 핸들러(우리가 작성한 코드)에게 최종적으로 전달합니다.

아래는 이 흐름을 보여주는 간단한 시퀀스 다이어그램입니다.

```mermaid
sequenceDiagram
    participant 수신 필터 as ReceiveFilter
    participant 파서 as RequestInfoParser (e.g., BasicRequestInfoParser)
    participant 요청 정보 객체 as RequestInfoObject (e.g., StringRequestInfo)
    participant AppSession as AppSession
    participant AppServer as AppServer
    participant 개발자 코드 as Handler Code

    수신 필터 ->> 파서: 파싱된 데이터 전달 (e.g., "/nick 멋진나")
    파서 ->> 요청 정보 객체: 데이터로 RequestInfo 객체 생성 (Key, Body, Parameters 채움)
    요청 정보 객체 -->> 파서: 생성된 객체 반환
    파서 -->> 수신 필터: RequestInfo 객체 반환
    수신 필터 -->> AppSession: RequestInfo 객체 반환 (Filter 메서드 결과)
    AppSession ->> AppServer: RequestInfo 객체 전달
    AppServer ->> 개발자 코드: NewRequestReceived 이벤트 발생 (Session, RequestInfo 전달)
    개발자 코드 ->> 요청 정보 객체: requestInfo.Key, requestInfo.Body 등 접근하여 사용

```

### 관련 코드 엿보기

`IRequestInfo` 인터페이스와 주요 구현 클래스들의 정의를 살펴보면 구조를 더 명확히 이해할 수 있습니다.

**`IRequestInfo` 인터페이스 ([SocketBase/Protocol/IRequestInfo.cs](SocketBase/Protocol/IRequestInfo.cs))**

모든 요청 정보의 기본 약속입니다. 오직 `Key` 속성만을 요구합니다.

```csharp
// 파일: SocketBase\Protocol\IRequestInfo.cs
namespace SuperSocketLite.SocketBase.Protocol
{
    /// <summary>
    /// 요청 정보 인터페이스
    /// </summary>
    public interface IRequestInfo
    {
        /// <summary>
        /// 이 요청의 키를 가져옵니다.
        /// </summary>
        string Key { get; }
    }
}
```

**`IRequestInfo<TRequestBody>` 인터페이스 ([SocketBase/Protocol/IRequestInfo.cs](SocketBase/Protocol/IRequestInfo.cs))**

`Key`와 함께 제네릭 타입 `TRequestBody`의 `Body` 속성을 요구합니다.

```csharp
// 파일: SocketBase\Protocol\IRequestInfo.cs (일부)
namespace SuperSocketLite.SocketBase.Protocol
{
    // ... (IRequestInfo 정의) ...

    /// <summary>
    /// 요청 정보 인터페이스 (본문 포함)
    /// </summary>
    /// <typeparam name="TRequestBody">요청 본문의 타입</typeparam>
    public interface IRequestInfo<TRequestBody> : IRequestInfo
    {
        /// <summary>
        /// 이 요청의 본문을 가져옵니다.
        /// </summary>
        TRequestBody Body { get; }
    }
}
```

**`RequestInfo<TRequestBody>` 클래스 ([SocketBase/Protocol/RequestInfo.cs](SocketBase/Protocol/RequestInfo.cs))**

`IRequestInfo<TRequestBody>` 인터페이스를 구현하는 기본적인 클래스입니다. `Key`와 `Body`를 저장합니다.

```csharp
// 파일: SocketBase\Protocol\RequestInfo.cs
namespace SuperSocketLite.SocketBase.Protocol
{
    /// <summary>
    /// RequestInfo 기본 클래스
    /// </summary>
    /// <typeparam name="TRequestBody">요청 본문의 타입</typeparam>
    public class RequestInfo<TRequestBody> : IRequestInfo<TRequestBody>
    {
        /// <summary>
        /// 생성자 (상속받는 클래스에서 사용 가능)
        /// </summary>
        protected RequestInfo() { }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="key">키.</param>
        /// <param name="body">본문.</param>
        public RequestInfo(string key, TRequestBody body)
        {
            Initialize(key, body);
        }

        /// <summary>
        /// 키와 본문으로 초기화합니다.
        /// </summary>
        /// <param name="key">키.</param>
        /// <param name="body">본문.</param>
        protected void Initialize(string key, TRequestBody body)
        {
            Key = key;
            Body = body;
        }

        /// <summary>
        /// 이 요청의 키를 가져옵니다. (set은 private)
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// 본문을 가져옵니다. (set은 private)
        /// </summary>
        public TRequestBody Body { get; private set; }
    }
}
```

**`StringRequestInfo` 클래스 ([SocketBase/Protocol/StringRequestInfo.cs](SocketBase/Protocol/StringRequestInfo.cs))**

`RequestInfo<string>`을 상속받아 `Parameters` 속성을 추가합니다.

```csharp
// 파일: SocketBase\Protocol\StringRequestInfo.cs
namespace SuperSocketLite.SocketBase.Protocol
{
    /// <summary>
    /// 문자열 타입 요청 정보
    /// </summary>
    public class StringRequestInfo : RequestInfo<string>
    {
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="key">키.</param>
        /// <param name="body">본문.</param>
        /// <param name="parameters">파라미터 배열.</param>
        public StringRequestInfo(string key, string body, string[] parameters)
            : base(key, body) // 부모 클래스 생성자 호출 (Key, Body 설정)
        {
            Parameters = parameters; // Parameters 속성 설정
        }

        /// <summary>
        /// 파라미터 배열을 가져옵니다. (set은 private)
        /// </summary>
        public string[] Parameters { get; private set; }

        // ... (GetFirstParam, 인덱서 등 추가적인 편의 메서드) ...
    }
}
```

이러한 클래스 구조를 통해 `IRequestInfo`는 다양한 형태의 요청 데이터를 일관성 있으면서도 유연하게 표현할 수 있습니다.

## 정리 및 다음 단계

이번 챕터에서는 [수신 필터 (IReceiveFilter)](04_수신_필터__ireceivefilter__.md)가 만들어낸 결과물이자, 우리의 서버 로직이 직접 다루게 되는 **요청 정보 (IRequestInfo)** 에 대해 배웠습니다. `IRequestInfo`는 마치 '잘 정리된 주문서'처럼, 클라이언트의 요청을 `Key`, `Body`, `Parameters` 등 구조화된 형태로 제공하여 서버 개발을 훨씬 편리하게 만들어 준다는 것을 알게 되었습니다. 특히 가장 많이 사용되는 `StringRequestInfo`를 예시로 들어, `NewRequestReceived` 이벤트 핸들러에서 어떻게 이 객체의 속성들을 활용하여 비즈니스 로직을 구현하는지 살펴보았습니다.

지금까지 우리는 서버를 설정하고([IServerConfig](01_서버_설정__iserverconfig__.md)), 서버를 운영하며([AppServer](02_애플리케이션_서버__appserver___appserverbase__.md)), 개별 클라이언트를 관리하고([AppSession](03_애플리케이션_세션__appsession__.md)), 들어오는 데이터를 해석하여([IReceiveFilter](04_수신_필터__ireceivefilter__.md)), 최종적으로 처리할 요청 정보([IRequestInfo](05_요청_정보__irequestinfo__.md))를 얻는 과정까지, SuperSocketLite의 애플리케이션 레벨 핵심 구성 요소들을 모두 살펴보았습니다!

하지만 이 모든 과정 뒤에는, 실제로 네트워크 연결을 관리하고 데이터를 주고받는 더 낮은 레벨의 구성 요소들이 묵묵히 일하고 있습니다. 다음 챕터부터는 조금 더 깊이 들어가서, `AppServer`와 `AppSession`의 기반이 되는 **소켓 서버 (SocketServer / SocketServerBase)** 와 **소켓 세션 (SocketSession)** 에 대해 알아볼 것입니다. 이들은 실제 네트워크 통신의 복잡한 부분을 담당하는 중요한 역할을 합니다.

**다음 챕터:** [Chapter 6: 소켓 서버 (SocketServer / SocketServerBase)](06_소켓_서버__socketserver___socketserverbase__.md)

---

Generated by [AI Codebase Knowledge Builder](https://github.com/The-Pocket/Tutorial-Codebase-Knowledge)