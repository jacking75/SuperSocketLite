# Chapter 4: 수신 필터 (ReceiveFilter)

[제3장: 요청 정보 (RequestInfo)](03_요청_정보__requestinfo_.md)에서 우리는 서버가 클라이언트의 요청을 이해하기 쉬운 형태로 변환한 '표준 주문서', 즉 `RequestInfo`에 대해 배웠습니다. `RequestInfo` 덕분에 서버는 "클라이언트가 무엇을 원하는가?"를 명확히 알 수 있었죠. 하지만, 클라이언트가 네트워크를 통해 보낸 원시 데이터(바이트 덩어리)가 어떻게 이 깔끔한 `RequestInfo` 객체로 변신하는 걸까요? 그 비밀을 푸는 열쇠가 바로 이번 장에서 배울 **수신 필터 (ReceiveFilter)**입니다.

## ReceiveFilter는 왜 필요할까요?

클라이언트와 서버는 네트워크를 통해 데이터를 주고받습니다. 이때 데이터는 연속된 바이트 스트림(byte stream) 형태로 전달됩니다. 그런데 이 바이트 스트림은 몇 가지 문제를 안고 있습니다.

1.  **메시지 경계 없음:** TCP/IP와 같은 일반적인 네트워크 프로토콜은 데이터가 '메시지' 단위로 구분되어 전달된다는 것을 보장하지 않습니다. 예를 들어 클라이언트가 "HELLO" 와 "WORLD" 두 메시지를 연달아 보내도, 서버는 "HE", "LLOWOR", "LD" 와 같이 조각나거나 합쳐진 형태로 데이터를 받을 수 있습니다.
2.  **불완전한 데이터:** 네트워크 상태에 따라 데이터가 한 번에 도착하지 않고 여러 조각으로 나뉘어 도착할 수 있습니다.

서버는 이렇게 뒤죽박죽 섞여 도착할 수 있는 바이트 스트림 속에서 의미 있는 메시지를 정확히 찾아내고, 그것이 완전한 형태인지 판단해야 합니다. 이것은 마치 여러 나라 언어로 뒤섞여 들려오는 주문 속에서 정확한 주문 내용을 파악해야 하는 레스토랑 직원과 같은 상황입니다.

바로 이 어려운 작업을 전문적으로 처리해주는 역할이 **ReceiveFilter**입니다. ReceiveFilter는 서버와 클라이언트 간의 약속된 통신 규칙(프로토콜)에 따라, 들어오는 바이트 스트림을 분석하여 완전한 메시지 단위를 식별하고, 이를 이전 장에서 배운 [요청 정보 (RequestInfo)](03_요청_정보__requestinfo_.md) 객체로 변환해줍니다.

**레스토랑 비유:**

ReceiveFilter는 마치 레스토랑의 **'전문 번역가 겸 주문 정리 담당자'**와 같습니다.
*   손님(클라이언트)이 다양한 언어(네트워크 바이트 스트림)로 주문합니다.
*   번역가(ReceiveFilter)는 손님의 말을 주의 깊게 듣고(데이터 수신 및 버퍼링), 완전한 주문 내용(하나의 완전한 메시지)이 파악될 때까지 기다립니다.
*   완전한 주문이 파악되면, 번역가는 이를 주방(서버 로직)에서 알아볼 수 있는 표준 주문서([RequestInfo](03_요청_정보__requestinfo_.md)) 양식으로 번역하고 정리하여 전달합니다.

덕분에 주방(서버 로직)은 복잡한 언어 해독 과정 없이, 잘 정리된 주문서만 보고 요리에 집중할 수 있습니다.

## ReceiveFilter 사용하기

ReceiveFilter는 직접 만들기보다는, SuperSocketLite가 제공하는 다양한 기본 필터 중 자신의 프로토콜에 맞는 것을 선택하여 사용하는 경우가 많습니다. 물론, 필요하다면 직접 만들 수도 있습니다. ReceiveFilter는 [애플리케이션 서버 (AppServer)](01_애플리케이션_서버__appserver_.md)를 설정할 때 지정합니다.

### ReceiveFilterFactory: 필터 생성 공장

[AppServer](01_애플리케이션_서버__appserver_.md)는 새로운 클라이언트가 연결될 때마다 해당 클라이언트([AppSession](02_애플리케이션_세션__appsession_.md))를 위한 *별도의* ReceiveFilter 인스턴스를 생성해야 합니다. 왜냐하면 각 클라이언트와의 통신 상태(예: 이전에 얼마만큼의 데이터를 받았는지)는 독립적으로 관리되어야 하기 때문입니다.

이러한 ReceiveFilter 인스턴스 생성을 책임지는 것이 바로 **ReceiveFilterFactory** (수신 필터 공장)입니다. `AppServer`의 생성자에 이 '공장'을 지정해주면, `AppServer`는 새 연결이 들어올 때마다 이 공장을 통해 필요한 ReceiveFilter를 만들어 사용합니다.

가장 일반적으로 사용되는 팩토리는 `DefaultReceiveFilterFactory`입니다. 이 팩토리는 지정된 ReceiveFilter 타입을 사용하여 간단하게 새 필터 인스턴스를 생성해줍니다.

```csharp
// MyServer.cs (이전 장 예제)
using SuperSocketLite.SocketBase;
using SuperSocketLite.SocketBase.Protocol;
using SuperSocketLite.SocketEngine.Protocol; // CommandLineReceiveFilter 사용 위해 추가

public class MyServer : AppServer<MySession, StringRequestInfo>
{
    // AppServer 생성자에서 ReceiveFilterFactory를 지정합니다.
    public MyServer()
        : base(new DefaultReceiveFilterFactory<CommandLineReceiveFilter, StringRequestInfo>())
        //      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //      여기에 '어떤 종류의 ReceiveFilter를 사용할지' 지정하는 공장을 전달합니다.
        //      DefaultReceiveFilterFactory<사용할 필터 타입, 생성될 RequestInfo 타입>
    {
        // ... 이벤트 핸들러 등록 ...
    }

    // ... (생략) ...
}
```
위 코드에서는 `CommandLineReceiveFilter`를 사용하고, 이 필터가 `StringRequestInfo`를 생성하도록 `DefaultReceiveFilterFactory`를 설정했습니다.

### 자주 사용되는 내장 ReceiveFilter

SuperSocketLite는 일반적인 통신 프로토콜을 위한 몇 가지 유용한 ReceiveFilter 구현을 미리 제공합니다.

1.  **`TerminatorReceiveFilter` (종료자 기반 필터):**
    *   **프로토콜:** 메시지가 특정 바이트 시퀀스(종료자, terminator)로 끝나는 규칙입니다. 예를 들어, 텍스트 기반 채팅 프로토콜에서 각 메시지가 줄바꿈 문자(`\r\n`)로 끝나는 경우가 여기에 해당합니다.
    *   **동작:** 종료자 바이트가 나타날 때까지 데이터를 계속 수신하고 버퍼링합니다. 종료자가 발견되면, 종료자까지의 데이터를 하나의 완전한 메시지로 간주하고 [RequestInfo](03_요청_정보__requestinfo_.md)를 생성합니다.
    *   **대표 예시:** `CommandLineReceiveFilter`
        *   `TerminatorReceiveFilter`를 상속받아 만들어졌습니다.
        *   기본적으로 공백(space) 또는 탭(tab)으로 구분되는 텍스트 명령어를 처리하며, 메시지는 줄바꿈(`\r\n`)으로 끝난다고 가정합니다.
        *   파싱 결과로 `StringRequestInfo`를 생성합니다. (`Key`: 첫 단어, `Body`: 나머지 전체, `Parameters`: 공백으로 나뉜 나머지 단어들)
        *   지난 장들의 에코 서버 예제에서 사용되었습니다.

    ```csharp
    // AppServer 설정 시 CommandLineReceiveFilter 사용 예시
    public class MyTextServer : AppServer<MySession, StringRequestInfo>
    {
        public MyTextServer()
            : base(new DefaultReceiveFilterFactory<CommandLineReceiveFilter, StringRequestInfo>())
        {
            // ...
        }
        // ...
    }
    ```

2.  **`FixedHeaderReceiveFilter` (고정 헤더 기반 필터):**
    *   **프로토콜:** 각 메시지가 고정된 크기의 '헤더(header)'로 시작하고, 이 헤더 안에 메시지 '본문(body)'의 길이가 포함되는 규칙입니다. 게임 서버나 바이너리 데이터 전송에 자주 사용됩니다.
    *   **동작:** 먼저 고정된 크기만큼의 헤더 데이터를 읽습니다. 헤더 데이터를 파싱하여 본문의 길이를 알아냅니다. 알아낸 길이만큼의 본문 데이터를 마저 읽습니다. 헤더와 본문 데이터가 모두 도착하면 이를 이용해 [RequestInfo](03_요청_정보__requestinfo_.md)를 생성합니다.
    *   **사용 예시:** (아래 '사용자 정의 ReceiveFilter 만들기' 참조) 직접 상속받아 `GetBodyLengthFromHeader`와 `ResolveRequestInfo` 메서드를 구현해야 합니다.

    ```csharp
    // AppServer 설정 시 사용자 정의 PacketReceiveFilter 사용 예시
    // (PacketReceiveFilter는 FixedHeaderReceiveFilter를 상속받아 구현했다고 가정)
    public class MyPacketServer : AppServer<MySession, PacketRequestInfo> // RequestInfo 타입도 변경됨
    {
        public MyPacketServer()
            : base(new DefaultReceiveFilterFactory<PacketReceiveFilter, PacketRequestInfo>())
        {
            // ...
        }
        // ...
    }
    ```

3.  **`FixedSizeReceiveFilter` (고정 크기 기반 필터):**
    *   **프로토콜:** 모든 메시지의 크기가 미리 정해진 고정 값인 규칙입니다.
    *   **동작:** 정해진 크기만큼의 데이터가 수신될 때까지 기다렸다가, 해당 데이터를 하나의 완전한 메시지로 간주하고 [RequestInfo](03_요청_정보__requestinfo_.md)를 생성합니다.
    *   **사용 예시:** 특정 길이의 센서 데이터나 고정 포맷 로그 수신 등에 사용될 수 있습니다. 사용하려면 상속 후 `ProcessMatchedRequest` 메서드를 구현해야 합니다.

4.  **`BeginEndMarkReceiveFilter` (시작/종료 표시자 기반 필터):**
    *   **프로토콜:** 메시지가 특정 '시작 표시자(begin mark)'로 시작하고 특정 '종료 표시자(end mark)'로 끝나는 규칙입니다. XML (`<tag>...</tag>`) 이나 특정 프레임 프로토콜에서 사용될 수 있습니다.
    *   **동작:** 시작 표시자를 찾으면 데이터를 버퍼링하기 시작하고, 종료 표시자를 찾으면 시작과 종료 표시자 사이의 데이터를 완전한 메시지로 간주하여 [RequestInfo](03_요청_정보__requestinfo_.md)를 생성합니다. 사용하려면 상속 후 `ProcessMatchedRequest` 메서드를 구현해야 합니다.

### 사용자 정의 ReceiveFilter 만들기

만약 여러분의 프로토콜이 SuperSocketLite의 내장 필터와 맞지 않거나, 더 복잡한 파싱 로직이 필요하다면 직접 ReceiveFilter를 만들 수 있습니다. 보통은 가장 유사한 내장 필터 클래스(예: `FixedHeaderReceiveFilter`, `TerminatorReceiveFilter`)를 **상속**받고, 필요한 메서드를 **오버라이드(override)**하는 방식으로 구현합니다.

가장 흔한 경우는 `FixedHeaderReceiveFilter`를 상속받아 바이너리 패킷 프로토콜을 처리하는 것입니다.

**예시: 간단한 게임 패킷 프로토콜용 ReceiveFilter**

다음과 같은 간단한 패킷 구조를 가정해 봅시다.
*   **헤더 (5 바이트):**
    *   전체 패킷 길이 (Body 길이 + 5) (2 바이트, short)
    *   패킷 ID (2 바이트, short)
    *   옵션 플래그 (1 바이트, sbyte)
*   **바디 (가변 길이):** 실제 데이터

이 프로토콜을 처리하는 `PacketReceiveFilter`를 만들어 보겠습니다. ([제3장: 요청 정보 (RequestInfo)](03_요청_정보__requestinfo_.md)에서 정의한 `GamePacketRequestInfo` 사용)

```csharp
// 네임스페이스 추가
using SuperSocketLite.SocketEngine.Protocol;
using SuperSocketLite.SocketBase.Protocol;
using SuperSocketLite.Common; // CloneRange 사용 위해
using System;

// 사용자 정의 RequestInfo (3장에서 정의함)
public class GamePacketRequestInfo : BinaryRequestInfo
{
    public short PacketID { get; private set; }
    public sbyte Flags { get; private set; }
    public const int HeaderSize = 5; // 헤더 크기 정의

    public GamePacketRequestInfo(short packetId, sbyte flags, byte[] body)
        : base(null, body) // BinaryRequestInfo는 Key가 없을 수 있음 (null 전달)
    {
        this.PacketID = packetId;
        this.Flags = flags;
    }
}

// FixedHeaderReceiveFilter를 상속받아 PacketReceiveFilter 구현
public class PacketReceiveFilter : FixedHeaderReceiveFilter<GamePacketRequestInfo>
{
    // 생성자: 부모 클래스에게 헤더 크기를 알려줍니다.
    public PacketReceiveFilter() : base(GamePacketRequestInfo.HeaderSize)
    {
    }

    // 헤더 데이터에서 Body 길이를 계산하여 반환하는 메서드 (오버라이드 필수)
    protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
    {
        // 헤더의 첫 2바이트가 전체 패킷 길이
        // (네트워크 바이트 순서(Big-Endian) 고려 필요 시 BitConverter.IsLittleEndian 확인 후 Array.Reverse)
        short totalLength = BitConverter.ToInt16(header, offset);
        // Body 길이는 전체 길이 - 헤더 길이
        return totalLength - GamePacketRequestInfo.HeaderSize;
    }

    // 헤더와 Body 데이터가 모두 준비되었을 때 호출되어 최종 RequestInfo를 생성 (오버라이드 필수)
    protected override GamePacketRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
    {
        // 헤더에서 PacketID (2바이트)와 Flags (1바이트) 추출
        // (네트워크 바이트 순서 고려 필요 시 BitConverter.IsLittleEndian 확인 후 Array.Reverse)
        short packetId = BitConverter.ToInt16(header.Array, header.Offset + 2);
        sbyte flags = (sbyte)header.Array[header.Offset + 4];

        // Body 데이터 복사 (bodyBuffer는 내부 버퍼일 수 있으므로 복사하는 것이 안전)
        byte[] body = bodyBuffer.CloneRange(offset, length);

        // 추출된 정보와 Body 데이터로 GamePacketRequestInfo 생성 및 반환
        return new GamePacketRequestInfo(packetId, flags, body);
    }
}
```

*   `PacketReceiveFilter`는 `FixedHeaderReceiveFilter<GamePacketRequestInfo>`를 상속받습니다. `GamePacketRequestInfo`는 이 필터가 생성할 [RequestInfo](03_요청_정보__requestinfo_.md)의 타입입니다.
*   생성자 `PacketReceiveFilter()`에서는 부모 생성자 `base(GamePacketRequestInfo.HeaderSize)`를 호출하여 헤더의 크기(여기서는 5)를 알려줍니다.
*   `GetBodyLengthFromHeader`: SuperSocketLite가 헤더 데이터를 성공적으로 읽으면 이 메서드를 호출합니다. 우리는 헤더 데이터(`header` 배열)를 보고 실제 Body의 길이를 계산하여 반환해야 합니다. 여기서는 헤더의 첫 2바이트에서 전체 길이를 읽고 헤더 크기를 빼서 Body 길이를 계산합니다.
*   `ResolveRequestInfo`: SuperSocketLite가 헤더와 Body 데이터를 모두 성공적으로 읽으면 이 메서드를 호출합니다. 우리는 전달받은 헤더(`header` ArraySegment)와 Body 데이터(`bodyBuffer` 배열, `offset`, `length`)를 사용하여 최종적인 `GamePacketRequestInfo` 객체를 만들어 반환합니다.

이렇게 만든 `PacketReceiveFilter`는 앞서 본 것처럼 `AppServer` 설정 시 `DefaultReceiveFilterFactory`에 지정하여 사용할 수 있습니다.

```csharp
// AppServer 설정 시 사용자 정의 PacketReceiveFilter 사용
public class MyGameServer : AppServer<MyGameSession, GamePacketRequestInfo>
{
    public MyGameServer()
        : base(new DefaultReceiveFilterFactory<PacketReceiveFilter, GamePacketRequestInfo>())
    {
        // ...
    }
    // ...
}
```

## ReceiveFilter 내부 동작 방식 (간략히)

클라이언트로부터 데이터가 도착했을 때 ReceiveFilter가 어떻게 작동하여 [RequestInfo](03_요청_정보__requestinfo_.md)를 만들어내는지 그 흐름을 살펴보겠습니다.

1.  **데이터 수신:** 클라이언트가 데이터를 보내면, 해당 클라이언트 연결을 관리하는 [소켓 세션 (SocketSession)](06_소켓_세션__socketsession_.md)이 네트워크로부터 원시 바이트 데이터를 수신합니다.
2.  **데이터 전달:** `SocketSession`은 수신된 데이터 조각(chunk)을 상위 레벨인 [애플리케이션 세션 (AppSession)](02_애플리케이션_세션__appsession_.md)에게 전달합니다.
3.  **`ReceiveFilter.Filter()` 호출:** `AppSession`은 자신이 가지고 있는 `ReceiveFilter` 인스턴스의 `Filter` 메서드를 호출하며, 수신된 데이터 조각과 그 위치 정보를 전달합니다.
4.  **필터링 및 파싱:** `ReceiveFilter`는 `Filter` 메서드 내에서 다음과 같은 작업을 수행합니다.
    *   **데이터 버퍼링:** 만약 이전 데이터 조각과 합쳐야 완전한 메시지를 만들 수 있다면, 내부 버퍼(예: `BufferSegments`)에 데이터를 추가합니다. (예: `TerminatorReceiveFilter`나 `FixedHeaderReceiveFilter`가 아직 완전한 메시지를 못 찾았을 경우)
    *   **프로토콜 규칙 검사:** 현재까지 수신된 데이터(내부 버퍼 포함)가 프로토콜 규칙에 따라 완전한 메시지를 형성하는지 확인합니다. (예: 종료자가 발견되었는지? 고정 헤더를 읽고 필요한 Body 데이터가 다 도착했는지?)
    *   **미완료 시 대기:** 아직 완전한 메시지가 아니라면, 아무것도 반환하지 않고 다음 데이터 조각을 기다립니다. 내부 버퍼 상태는 유지됩니다.
    *   **완료 시 파싱:** 완전한 메시지가 식별되면, 해당 메시지 데이터를 프로토콜에 맞게 파싱합니다. (예: `CommandLineReceiveFilter`는 공백으로 분리, `FixedHeaderReceiveFilter`는 헤더와 바디 분리)
    *   **RequestInfo 생성:** 파싱된 결과를 바탕으로 적절한 [RequestInfo](03_요청_정보__requestinfo_.md) 객체(예: `StringRequestInfo`, `GamePacketRequestInfo`)를 생성합니다.
    *   **RequestInfo 반환 및 소비 정보 전달:** 생성된 `RequestInfo` 객체를 반환합니다. 동시에, 이번 `Filter` 호출에서 얼마만큼의 데이터를 소비했는지 알려주기 위해 `rest` 파라미터에 남은 데이터의 길이를 설정합니다. (완전한 메시지 하나를 처리하고도 데이터가 남는 경우)
5.  **`NewRequestReceived` 이벤트 발생:** `AppSession`이 `ReceiveFilter`로부터 `RequestInfo` 객체를 받으면 (null이 아니면), [AppServer](01_애플리케이션_서버__appserver_.md)의 `NewRequestReceived` 이벤트를 발생시킵니다. 이때 `AppSession` 자신과 방금 생성된 `RequestInfo` 객체를 이벤트 데이터로 전달합니다.
6.  **남은 데이터 처리:** 만약 `Filter` 메서드가 `rest` 값으로 0보다 큰 값을 반환했다면, 이는 현재 데이터 조각에 처리하고 남은 데이터가 있다는 의미입니다. `AppSession`은 이 남은 데이터를 가지고 다시 `ReceiveFilter.Filter()` 메서드를 호출하여 다음 메시지를 처리하려고 시도합니다.

이 과정을 간단한 순서도로 나타내면 다음과 같습니다.

```mermaid
sequenceDiagram
    participant C as 클라이언트
    participant SockSess as 소켓 세션
    participant AppSess as AppSession
    participant RecvFilter as ReceiveFilter
    participant AppSrv as AppServer

    C->>+SockSess: 데이터 전송 (바이트 조각)
    SockSess->>+AppSess: 데이터 조각 전달
    AppSess->>+RecvFilter: Filter(데이터 조각) 호출
    RecvFilter->>RecvFilter: 데이터 버퍼링 및 프로토콜 검사
    alt 완전한 메시지 발견
        RecvFilter->>RecvFilter: 메시지 파싱 및 RequestInfo 생성
        RecvFilter-->>-AppSess: RequestInfo 객체 반환 (rest=남은 데이터 길이)
        AppSess->>+AppSrv: NewRequestReceived 이벤트 발생 (세션, RequestInfo 전달)
        AppSrv-->>-AppSess: (이벤트 핸들러 실행)
        AppSess->>AppSess: 남은 데이터(rest > 0)로 Filter 재호출
    else 아직 미완료
        RecvFilter-->>-AppSess: null 반환 (rest=0)
    end
    AppSess-->>-SockSess: 처리 완료
```

### 코드 레벨에서 살펴보기

모든 ReceiveFilter는 `IReceiveFilter<TRequestInfo>` 인터페이스를 구현해야 합니다. 이 인터페이스의 핵심은 `Filter` 메서드입니다.

```csharp
// 파일: SuperSocketLite\SocketBase\Protocol\IReceiveFilter.cs (일부)

namespace SuperSocketLite.SocketBase.Protocol;

public interface IReceiveFilter<TRequestInfo> where TRequestInfo : IRequestInfo
{
    // 수신된 데이터를 필터링하여 RequestInfo를 만듭니다.
    TRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest);
    // readBuffer: 수신된 데이터가 담긴 버퍼
    // offset: 버퍼 내에서 이번에 수신된 데이터의 시작 위치
    // length: 이번에 수신된 데이터의 길이
    // toBeCopied: 버퍼 데이터를 복사해야 하는지 여부 (내부 처리용)
    // rest (out): 필터가 데이터를 처리하고 남은 바이트 수 (다음 처리를 위해)
    // 반환값: 완성된 RequestInfo 객체 (아직 미완성이면 null)

    // 내부 버퍼에 남아있는 데이터 크기
    int LeftBufferSize { get; }

    // 다음 번 Filter 호출 시 사용될 필터 (필터 전환 기능 지원)
    IReceiveFilter<TRequestInfo> NextReceiveFilter { get; }

    // 필터 상태 초기화
    void Reset();

    // 현재 필터 상태 (예: 정상, 오류)
    FilterState State { get; }
}
```
`Filter` 메서드는 ReceiveFilter의 심장과 같습니다. 여기서 모든 프로토콜 분석과 파싱이 이루어집니다.

`FixedHeaderReceiveFilter`와 같은 기본 클래스를 상속받으면 `Filter` 메서드를 직접 구현할 필요는 없습니다. 대신, `GetBodyLengthFromHeader`와 `ResolveRequestInfo` 같은 추상 메서드를 구현하여 프로토콜의 특정 부분만 처리하면 됩니다. `FixedHeaderReceiveFilter`의 내부 `Filter` 로직이 헤더 읽기, Body 길이 계산, Body 읽기 등의 과정을 알아서 처리해줍니다.

```csharp
// 파일: SuperSocketLite\SocketEngine\Protocol\FixedHeaderReceiveFilter.cs (개념적 흐름)

public abstract class FixedHeaderReceiveFilter<TRequestInfo> : ...
{
    private bool m_FoundHeader = false; // 헤더를 찾았는지 여부
    private int m_BodyLength;       // 계산된 Body 길이
    // ... 기타 상태 변수 ...

    // 생성자에서 헤더 크기(Size)를 설정 받음
    protected FixedHeaderReceiveFilter(int headerSize) : base(headerSize) { }

    // Filter 메서드 (SuperSocketLite 내부에서 호출됨)
    public override TRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
    {
        if (!m_FoundHeader) // 아직 헤더를 못 찾았다면
        {
            // 1. 고정 크기(Size)만큼 헤더 데이터를 읽으려고 시도
            //    (FixedSizeReceiveFilter의 로직 활용)
            //    데이터가 충분하면 ProcessMatchedRequest 호출, 부족하면 null 반환
            return base.Filter(readBuffer, offset, length, toBeCopied, out rest);
        }
        else // 헤더는 이미 찾았고, Body 데이터를 읽는 중이라면
        {
            // 3. 필요한 Body 길이(m_BodyLength)만큼 데이터를 읽으려고 시도
            //    데이터가 충분하면 헤더와 Body 데이터로 ResolveRequestInfo 호출
            //    데이터가 부족하면 내부 버퍼(m_BodyBuffer)에 저장하고 null 반환
            // ... Body 처리 로직 ...
        }
    }

    // 헤더 데이터가 성공적으로 읽혔을 때 호출됨 (FixedSizeReceiveFilter 로직에 의해)
    protected override TRequestInfo ProcessMatchedRequest(byte[] buffer, int offset, int length, bool toBeCopied)
    {
        m_FoundHeader = true;
        // 2. 구현된 GetBodyLengthFromHeader 호출하여 Body 길이 계산
        m_BodyLength = GetBodyLengthFromHeader(buffer, offset, length);
        // 헤더 데이터 저장 (m_Header)
        // ...
        // 만약 Body 길이가 0이면 즉시 ResolveRequestInfo 호출, 아니면 null 반환하고 Body 읽기 시작
        return NullRequestInfo; // Body를 더 읽어야 함
    }

    // 헤더에서 Body 길이를 계산 (구현 필수)
    protected abstract int GetBodyLengthFromHeader(byte[] header, int offset, int length);

    // 헤더와 Body가 모두 준비되었을 때 RequestInfo 생성 (구현 필수)
    protected abstract TRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length);
}
```
위 코드 흐름처럼, `FixedHeaderReceiveFilter`는 헤더 읽기 -> Body 길이 계산 -> Body 읽기 -> `RequestInfo` 생성의 과정을 체계적으로 처리하며, 개발자는 자신의 프로토콜에 맞는 계산 및 생성 로직만 구현하면 됩니다.

마지막으로, [AppSession](02_애플리케이션_세션__appsession_.md)이 초기화될 때 `ReceiveFilterFactory`를 사용하여 각 세션에 맞는 ReceiveFilter를 생성하는 부분을 살펴보겠습니다.

```csharp
// 파일: SuperSocketLite\SocketBase\AppSession.cs (Initialize 메서드 일부)

public virtual void Initialize(IAppServer<TAppSession, TRequestInfo> appServer, ISocketSession socketSession)
{
    // ... AppServer, SocketSession 등 설정 ...

    var castedAppServer = (AppServerBase<TAppSession, TRequestInfo>)appServer;
    // AppServer에 설정된 ReceiveFilterFactory를 가져옵니다.
    var filterFactory = castedAppServer.ReceiveFilterFactory;

    // Factory를 사용하여 이 세션을 위한 ReceiveFilter 인스턴스를 생성합니다.
    m_ReceiveFilter = filterFactory.CreateFilter(appServer, this, socketSession.RemoteEndPoint);
    //                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    // 필터 초기화 (필요한 경우)
    var filterInitializer = m_ReceiveFilter as IReceiveFilterInitializer;
    if (filterInitializer != null)
        filterInitializer.Initialize(castedAppServer, this);

    // ... SocketSession 초기화 등 ...
}
```
`AppSession`이 시작될 때 `AppServer`에 등록된 `ReceiveFilterFactory`를 통해 `CreateFilter` 메서드가 호출되어, 해당 세션 전용 `ReceiveFilter`가 만들어지고 `m_ReceiveFilter` 멤버 변수에 할당됩니다. 이후 이 세션으로 들어오는 모든 데이터는 이 `m_ReceiveFilter` 인스턴스에 의해 처리됩니다.

## 결론

이번 장에서는 클라이언트로부터 오는 원시 바이트 스트림을 서버가 이해할 수 있는 의미 있는 [요청 정보 (RequestInfo)](03_요청_정보__requestinfo_.md)로 변환하는 핵심 역할, **수신 필터 (ReceiveFilter)**에 대해 배웠습니다. ReceiveFilter는 통신 프로토콜을 정의하고 구현하는 곳이며, SuperSocketLite는 다양한 내장 필터(종료자 기반, 고정 헤더 기반 등)를 제공하여 일반적인 프로토콜을 쉽게 처리할 수 있도록 돕습니다. 또한, 필요에 따라 내장 필터를 상속받아 사용자 정의 필터를 만들어 복잡한 프로토콜도 처리할 수 있음을 확인했습니다.

ReceiveFilter는 `AppServer` 설정 시 `ReceiveFilterFactory`를 통해 지정되며, 각 [AppSession](02_애플리케이션_세션__appsession_.md)마다 독립적인 인스턴스가 생성되어 상태를 관리합니다.

이제 우리는 SuperSocketLite 애플리케이션의 주요 구성 요소인 [AppServer](01_애플리케이션_서버__appserver_.md)(서버 총괄), [AppSession](02_애플리케이션_세션__appsession_.md)(클라이언트 연결 관리), [RequestInfo](03_요청_정보__requestinfo_.md)(파싱된 요청), 그리고 ReceiveFilter(프로토콜 처리)에 대해 모두 배웠습니다. 이들은 애플리케이션 레벨에서 서버 로직을 구축하는 데 필요한 핵심 요소들입니다.

다음 장부터는 조금 더 깊이 들어가, 이러한 애플리케이션 레벨 구성 요소들이 실제로 어떻게 네트워크 통신을 수행하는지 그 기반 구조를 살펴보겠습니다. 첫 번째로 **[제5장: 소켓 서버 (SocketServer)](05_소켓_서버__socketserver_.md)**에 대해 알아볼 것입니다. SocketServer는 실제 네트워크 포트를 열고 클라이언트 연결을 받아들이는 저수준(low-level) 작업을 담당하는 중요한 구성 요소입니다.

---

Generated by [AI Codebase Knowledge Builder](https://github.com/The-Pocket/Tutorial-Codebase-Knowledge)