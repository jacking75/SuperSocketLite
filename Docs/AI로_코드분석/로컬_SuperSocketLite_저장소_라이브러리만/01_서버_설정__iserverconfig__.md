# Chapter 1: 서버 설정 (IServerConfig)

SuperSocketLite를 사용한 네트워크 애플리케이션 개발 여정에 오신 것을 환영합니다! 첫 번째 단계로, 서버의 행동 방식을 결정하는 가장 기본적인 요소인 **서버 설정(Server Configuration)** 에 대해 알아보겠습니다.

## 왜 서버 설정이 필요할까요?

여러분이 멋진 온라인 게임 서버나 실시간 채팅 애플리케이션을 만든다고 상상해 보세요. 서버를 처음 만들 때는 개발용 컴퓨터에서 특정 포트 번호(예: 8080)로 테스트하고, 동시에 접속할 수 있는 사용자 수도 10명 정도로 제한할 수 있습니다.

하지만 실제 서비스를 시작할 때는 어떻게 해야 할까요? 아마도 다른 포트 번호(예: 80 또는 443)를 사용해야 하고, 훨씬 더 많은 사용자(예: 1000명 이상)를 동시에 처리해야 할 것입니다. 또한, 보안을 위해 암호화(SSL/TLS)를 적용해야 할 수도 있습니다.

이럴 때마다 서버 프로그램 코드를 직접 수정하고 다시 컴파일하는 것은 매우 번거롭고 위험합니다. 바로 이 문제를 해결하기 위해 **서버 설정**이 필요합니다. 서버 설정은 마치 스마트폰의 설정 메뉴와 같습니다. 앱을 다시 설치하지 않고도 Wi-Fi 비밀번호를 바꾸거나 화면 밝기를 조절하는 것처럼, 서버 설정을 통해 코드 변경 없이 서버의 동작 방식을 유연하게 변경할 수 있습니다.

SuperSocketLite에서는 이러한 설정을 `IServerConfig` 인터페이스를 통해 관리합니다.

## `IServerConfig`란 무엇인가요?

`IServerConfig`는 SuperSocketLite 서버 인스턴스가 어떻게 작동해야 하는지에 대한 모든 규칙과 지침을 담고 있는 **설계도** 또는 **운영 매뉴얼**과 같습니다. 인터페이스(Interface)는 특정 기능을 수행하기 위해 어떤 것들이 필요한지를 정의하는 약속이며, `IServerConfig`는 서버 설정을 위해 다음과 같은 정보들이 필요하다고 약속합니다.

*   **서버 이름 (Name)**: 여러 서버 인스턴스를 운영할 때 구분하기 위한 이름입니다.
*   **IP 주소 (Ip)**: 서버가 어떤 네트워크 인터페이스에서 클라이언트 연결을 기다릴지 지정합니다. (예: 모든 IP에서 기다리려면 "Any" 또는 "0.0.0.0", 특정 IP만 지정할 수도 있습니다.)
*   **포트 번호 (Port)**: 클라이언트가 서버에 접속하기 위해 사용하는 고유한 번호입니다. (예: 80, 443, 8080)
*   **최대 동시 접속자 수 (MaxConnectionNumber)**: 서버가 동시에 처리할 수 있는 최대 클라이언트 연결 수를 제한합니다. 너무 많은 연결로 서버가 느려지는 것을 방지합니다.
*   **버퍼 크기 (ReceiveBufferSize, SendBufferSize)**: 데이터를 주고받을 때 사용할 임시 저장 공간의 크기를 정합니다.
*   **세션 타임아웃 (IdleSessionTimeOut)**: 클라이언트가 아무런 활동 없이 일정 시간 동안 연결되어 있으면 자동으로 연결을 끊도록 설정합니다. 서버 자원을 효율적으로 관리하는 데 도움이 됩니다.
*   **보안 설정 (Security, Certificate)**: SSL/TLS 같은 암호화 통신을 사용할지 여부와 관련 인증서 정보를 설정합니다.
*   **리스너 설정 (Listeners)**: 하나의 서버가 여러 포트나 IP 주소에서 동시에 연결을 기다리도록 설정할 수 있습니다.

이 외에도 다양한 설정 값들이 `IServerConfig`를 통해 정의됩니다.

## `IServerConfig` 사용하기 (기본 설정 예시)

실제로 서버를 설정할 때는 `IServerConfig` 인터페이스를 구현하는 클래스의 객체를 생성하고 그 속성값을 지정합니다. SuperSocketLite는 기본 구현 클래스인 `ServerConfig`를 제공합니다.

가장 간단한 방법은 C# 코드 내에서 직접 `ServerConfig` 객체를 만들고 설정하는 것입니다. 예를 들어, 'MyEchoServer'라는 이름으로, 8080 포트를 사용하고, 최대 50명의 동시 접속자를 허용하는 서버를 설정해 보겠습니다.

```csharp
using SuperSocketLite.SocketBase.Config;

// ServerConfig 객체 생성
var serverConfig = new ServerConfig
{
    Name = "MyEchoServer", // 서버 이름 설정
    Ip = "Any",            // 모든 IP 주소에서 연결 허용
    Port = 8080,           // 사용할 포트 번호 설정
    MaxConnectionNumber = 50, // 최대 동시 접속자 수 설정
    Mode = SocketMode.Tcp,  // TCP 프로토콜 사용
    // 다른 설정들은 기본값을 사용합니다.
    // 예를 들어, IdleSessionTimeOut 기본값은 300초(5분)입니다.
};

// 이제 이 serverConfig 객체를 사용하여 서버 인스턴스를 초기화합니다.
// (서버 초기화는 다음 챕터에서 자세히 다룹니다.)
```

위 코드에서는 `ServerConfig` 객체를 만들고 `Name`, `Ip`, `Port`, `MaxConnectionNumber`, `Mode` 속성만 직접 설정했습니다. 나머지 설정들은 `ServerConfig` 클래스에 미리 정의된 기본값(Default Value)을 사용하게 됩니다. 예를 들어, `IdleSessionTimeOut`의 기본값은 300초입니다.

이렇게 생성된 `serverConfig` 객체는 나중에 [애플리케이션 서버 (AppServer / AppServerBase)](02_애플리케이션_서버__appserver___appserverbase__.md)를 생성하고 시작할 때 전달되어, 서버가 이 설정에 따라 동작하도록 지시하는 역할을 합니다.

## 주요 설정 속성 살펴보기

`IServerConfig` (그리고 그 구현체인 `ServerConfig`)에는 많은 설정 속성이 있지만, 몇 가지 중요한 것들을 더 자세히 살펴보겠습니다.

*   `Ip` (string): 서버가 클라이언트 연결을 수신 대기할 IP 주소입니다. 보통 "Any" (모든 네트워크 인터페이스) 또는 "0.0.0.0" (IPv4의 모든 인터페이스), "::" (IPv6의 모든 인터페이스)를 사용하거나, 특정 네트워크 카드의 IP 주소를 지정할 수 있습니다.
*   `Port` (int): 서버가 사용할 TCP/UDP 포트 번호입니다. 0 ~ 65535 사이의 값이며, 일반적으로 1024 미만은 시스템에서 예약된 경우가 많으므로 피하는 것이 좋습니다.
*   `MaxConnectionNumber` (int): 서버에 동시에 연결될 수 있는 클라이언트의 최대 개수입니다. 서버의 성능과 자원에 맞춰 적절히 설정해야 합니다. 기본값은 100입니다.
*   `ReceiveBufferSize` / `SendBufferSize` (int): 데이터를 수신하거나 발송할 때 사용하는 내부 버퍼의 크기(바이트 단위)입니다. 네트워크 상태와 주고받는 데이터의 평균 크기에 따라 조절할 수 있습니다. 기본값은 각각 4096, 2048입니다.
*   `IdleSessionTimeOut` (int): 클라이언트가 마지막으로 데이터를 보내거나 받은 후, 아무런 활동 없이 연결이 유지될 수 있는 최대 시간(초 단위)입니다. 이 시간이 지나면 서버는 해당 클라이언트 연결을 자동으로 종료합니다. 기본값은 300초 (5분)입니다.
*   `Security` (string): 통신 보안 수준을 설정합니다. "None" (기본값, 암호화 없음), "Tls", "Ssl3" 등을 지정할 수 있습니다. 보안 통신을 사용하려면 `Certificate` 속성도 함께 설정해야 합니다.
*   `Certificate` ([ICertificateConfig](SocketBase/Config/ICertificateConfig.cs)): SSL/TLS 통신에 사용할 서버 인증서 정보를 설정합니다. 인증서 파일 경로, 비밀번호, 저장소 위치 등을 지정할 수 있습니다. ([CertificateConfig.cs](SocketBase/Config/CertificateConfig.cs) 참고)
*   `Listeners` (IEnumerable<[IListenerConfig](SocketBase/Config/IListenerConfig.cs)>): 기본 `Ip`와 `Port` 설정 외에 추가적인 리스닝 주소/포트를 설정할 때 사용합니다. 예를 들어, 내부망용 포트와 외부망용 포트를 동시에 열어두는 경우에 유용합니다. ([ListenerConfig.cs](SocketBase/Config/ListenerConfig.cs) 참고)
*   `Mode` (SocketMode): 통신 프로토콜을 지정합니다. `SocketMode.Tcp` (기본값) 또는 `SocketMode.Udp`를 설정할 수 있습니다.

**참고:** 실제 프로젝트에서는 이러한 설정들을 코드에 직접 작성하기보다는, 별도의 설정 파일(예: `app.config`, `YourApp.exe.config` (XML 형식) 또는 `appsettings.json` (JSON 형식))에 저장하고, SuperSocketLite가 시작될 때 이 파일을 읽어 `ServerConfig` 객체를 자동으로 생성하도록 하는 것이 일반적입니다. 이렇게 하면 코드를 변경하지 않고 설정 파일만 수정하여 서버 동작을 바꿀 수 있어 더욱 편리합니다.

## 내부 동작 방식 (간단히 엿보기)

`IServerConfig`는 인터페이스이므로 직접적인 동작은 없습니다. 실제 동작은 이 인터페이스를 구현하는 클래스(예: `ServerConfig`)와 SuperSocketLite의 다른 구성 요소들이 상호작용하는 방식에 따라 결정됩니다.

일반적으로 서버 애플리케이션이 시작될 때 다음과 같은 과정을 거칩니다.

1.  **설정 로드:** 애플리케이션은 코드에서 직접 `ServerConfig` 객체를 생성하거나, 설정 파일(XML, JSON 등)에서 설정 정보를 읽어와 `ServerConfig` 객체를 만듭니다.
2.  **서버 초기화:** 생성된 `ServerConfig` 객체를 사용하여 [애플리케이션 서버 (AppServer / AppServerBase)](02_애플리케이션_서버__appserver___appserverbase__.md) 인스턴스를 초기화합니다.
3.  **설정 적용:** AppServer는 전달받은 `ServerConfig` 객체의 값들을 읽어 자신의 내부 동작 방식을 설정합니다. 예를 들어, 지정된 `Port` 번호로 리스닝 소켓을 열고, `MaxConnectionNumber`를 초과하는 연결은 받지 않도록 설정합니다.

아래는 설정 파일로부터 설정을 로드하는 과정을 간단히 나타낸 순서 다이어그램입니다.

```mermaid
sequenceDiagram
    participant 사용자 코드 as User Code
    participant 설정 로더 as Config Loader (SuperSocket)
    participant 설정 파일 as Config File (e.g., app.config)
    participant 서버 설정 객체 as ServerConfig Object

    사용자 코드 ->> 설정 로더: 설정 로드 요청 (예: 파일 경로 전달)
    설정 로더 ->> 설정 파일: 파일 내용 읽기
    설정 파일 -->> 설정 로더: 설정 데이터 (XML/JSON 등) 반환
    설정 로더 ->> 서버 설정 객체: 데이터로 ServerConfig 객체 생성 및 속성 채우기
    서버 설정 객체 -->> 설정 로더: 생성된 ServerConfig 객체 반환
    설정 로더 -->> 사용자 코드: ServerConfig 객체 전달
end
```

SuperSocketLite 내부에서는 `ServerConfig.cs` ([SocketBase/Config/ServerConfig.cs](SocketBase/Config/ServerConfig.cs)) 클래스가 `IServerConfig` 인터페이스의 실제 구현을 제공합니다. 이 클래스에는 다양한 설정 속성과 그 기본값들이 정의되어 있습니다.

```csharp
// ServerConfig.cs의 일부 발췌 (간략화됨)
namespace SuperSocketLite.SocketBase.Config
{
    [Serializable]
    public partial class ServerConfig : IServerConfig
    {
        // 기본값 상수 정의
        public const int DefaultMaxConnectionNumber = 100;
        public const int DefaultIdleSessionTimeOut = 300; // 300초 = 5분
        // ... 기타 기본값들 ...

        // 생성자: 기본값들로 속성 초기화
        public ServerConfig()
        {
            Security = "None";
            MaxConnectionNumber = DefaultMaxConnectionNumber;
            Mode = SocketMode.Tcp;
            IdleSessionTimeOut = DefaultIdleSessionTimeOut;
            // ... 기타 속성들의 기본값 할당 ...
        }

        // IServerConfig 인터페이스의 속성들 구현
        public string Name { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public int MaxConnectionNumber { get; set; }
        public int IdleSessionTimeOut { get; set; }
        public string Security { get; set; }
        public ICertificateConfig Certificate { get; set; }
        public IEnumerable<IListenerConfig> Listeners { get; set; }
        public SocketMode Mode { get; set; }
        // ... 기타 많은 속성들 ...
    }
}
```

이 `ServerConfig` 클래스는 서버의 다양한 운영 파라미터를 담는 컨테이너 역할을 하며, 코드 또는 설정 파일을 통해 값을 채울 수 있도록 설계되었습니다.

## 정리 및 다음 단계

이번 챕터에서는 SuperSocketLite 서버의 동작을 제어하는 핵심 요소인 `IServerConfig`에 대해 배웠습니다. `IServerConfig`는 서버의 포트 번호, 최대 접속자 수, 타임아웃 시간 등 다양한 운영 규칙을 정의하는 **설계도**와 같으며, 이를 통해 코드 변경 없이 서버의 행동을 유연하게 조정할 수 있다는 것을 알게 되었습니다. 또한, C# 코드로 직접 `ServerConfig` 객체를 생성하고 기본 설정을 지정하는 간단한 예시를 살펴보았습니다.

이제 서버를 어떻게 설정하는지 알았으니, 다음 단계는 실제로 이 설정을 사용하여 클라이언트 연결을 받아들이고 관리하는 서버의 본체, 즉 **애플리케이션 서버**에 대해 알아보는 것입니다.

다음 챕터에서 만나요!

**다음 챕터:** [Chapter 2: 애플리케이션 서버 (AppServer / AppServerBase)](02_애플리케이션_서버__appserver___appserverbase__.md)

---

Generated by [AI Codebase Knowledge Builder](https://github.com/The-Pocket/Tutorial-Codebase-Knowledge)