# Tutorial
여기에는 있는 서버를 순서대로 만들어 보면서 SuperSocketLite 사용 방법을 배운다.  
각 서버 프로젝트를 빌드하면 00_server_bins 디렉토리에 출력한다.  
    
  
## EchoServer
- 가장 간단한 서버이다.
- 클라이언트가 보낸 것을 그대로 클라이언트에게 보낸다.
- 간단하게 SuperSocketLite를 애플리케이션에서 어떻게 사용하는지 배운다.  
  
빌드 후 run_EchoServer.bat 배치 파일로 실행한다.
    
  
  
## EchoServerEx
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