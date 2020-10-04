# Tutorial
여기에는 있는 서버를 순서대로 만들어 보면서 SuperSocketLite 사용 방법을 배운다.  
각 서버 프로젝트를 빌드하면 00_server_bins 디렉토리에 출력한다.  
    
  
## 중요
- 네트워크 이벤트 중 동일 세션이라도 `NewSessionConnected` 와 `NewRequestReceived` 다른 스레드에서 동시에 발생할 수 있다. 즉 클라이언트에서 접속하자말자 바로 패킷을 보내면 `NewSessionConnected`을 처리하는 중에 `NewRequestReceived`이 호출될 수 있다.
  
  
  
## EchoServer
- 가장 간단한 서버이다.
- 클라이언트가 보낸 것을 그대로 클라이언트에게 보낸다.
- 간단하게 SuperSocketLite를 애플리케이션에서 어떻게 사용하는지 배운다.  
  
빌드 후 run_EchoServer.bat 배치 파일로 실행한다.
    
  
  
## EchoServerEx
- [유튜브 영상](https://youtu.be/ZgzMuHE43hU )
- EchoServer를 좀 더 고도화 한 것이다.
- 서버 옵션을 프로그램 실행 시 인자로 받는다.
- NLog를 사용한다.
- SuperSocketLite 프로젝트를 참조하지 않고, SuperSocketLite를 빌드한 dll 파일을 참조한다.
  
### 프로젝트에 추가할 것 
- 00_superSocketLite_libs 디렉토리에 있는 dll을 추가한다.
- Nuget 추가 
    - CommandLineParser
	- NLog.Extensions.Logging
	- System.Configuration.ConfigurationManager
  
빌드 후 run_EchoServerEx.bat 배치 파일로 실행한다. 
  
  
  
## MultiPortServer  
서버가 복수의 port 번호를 사용하는 경우에 대한 예제이다.  
  
빌드 후 run_MultiPortServer.bat 배치 파일로 실행한다. 
  
  
## ChatServer
- [유튜브 영상](https://youtu.be/eiwvQ8NV2h8 )
- 채팅 서버
- 패킷 처리는 1개의 스레드만으로 처리한다.
  
  
## ChatServerEx  
- 채팅 서버
- ChatServer와 달리 패킷 처리를 멀티스레드로 한다는 것만 빼고는 같음