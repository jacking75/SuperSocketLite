# 게임 서버 템플릿
이 디렉토리에 있는 게임 서버 프로젝트를 사용하여 빠르게 게임 서버를 개발하도록 한다. 
     
    
## GameServer_01
- 에코 기능만 구현 되어 있는 서버이다.
- Logger 라이브러리는 ZLogger를 사용하고 있다.
   

## GameServer_01_GenericHost
- `GameServer_01`에 `GenericHost` 기능을 추가해서 만든 것이다.
- 빌드 후 EchoServer_GenericHost.bat 배치 파일로 실행한다. 
   
[Generic Host(일반 호스트) 소개 및 사용](https://jacking75.github.io/NET_GenericHost/)  | [MS Docs](https://learn.microsoft.com/ko-kr/dotnet/core/extensions/generic-host?tabs=appbuilder)     
    
  
  
## GameServer_02
- 패킷 데이터 직렬화 라이브러리로 `MemoryPack`를 사용한다
- 테스트용 클라이언트는 `GameServer_MemoryPack`을 사용한다.  
    
  
## GameServer_03
- 패킷 데이터 직렬화 라이브러리로 `Protocol.Buf`를 사용한다
- 만들지는 않았다. `GameServer_02` 와 `TestProtocolBuffer`(Test 디렉토리에 있다)을 참고하면 된다.