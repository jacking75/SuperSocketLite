using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;


// asyn-await를 사용한 대량의 게임 업데이트 처리
/*
- 멀티스레드로 update가 호출 되고 있나? sleep 걸어보면 될듯. => 멀티 스레드로 동작
- 일반적인 스레드풀 방식보다 CPU를 덜 사용. 50% 정도
- 간격을 짧게 주면 맞지 않음.간격을 크게 주면 거의 맞음
 - 16ms 하면 16~30 사이
 - 200ms 하면 20~208 정도로 거의 맞음(1000개 실행)
 - 100ms 하면 110~120 정도(1000개 실행)
*/
namespace GameServer;

public class GameLogic
{
    UInt32 _index = 0;
    bool _isRunable = false;
    ConcurrentQueue<GameMessage> _msgQueue = new ();


    public void Init(UInt32 index)
    {
        _index = index;
    }

    public void Clear()
    {
    }

    public void Start()
    {
        _isRunable = true;
        Task.Run(() => Update());
    }

    public void End()
    {
        _isRunable = false;
    }

    public void AddMessage(UInt16 msgId, byte[] msgData)
    {
        _msgQueue.Enqueue(new GameMessage(msgId, msgData));
    }

    public async Task<int> Update()
    {
        MainServer.MainLogger.Debug($"[GameLogic-Update] Start. Index:{_index}");

        while (_isRunable)
        {
            MainServer.MainLogger.Debug($"[GameLogic-Update] Call. Index:{_index}, [{DateTime.Now.Millisecond}]");

            if (_msgQueue.TryDequeue(out var gameMsg))
            {
                MainServer.MainLogger.Debug($"[GameLogic-Update] id: {gameMsg.Id}. Index:{_index}");

                if (gameMsg.Id == 0)
                {
                    return 1;
                }
            }

            //System.Threading.Thread.Sleep(3000);

            //MainServer.MainLogger.Debug($"[GameLogic-Update] Next Frame... Index:{Index}");
            await Task.Delay(100); // 
        }

        MainServer.MainLogger.Debug($"[GameLogic-Update] End. Index:{_index}");
        return 0;
    }
    
}


public struct GameMessage
{
    public GameMessage(UInt16 msgId, byte[] msgData)
    {
        Id = msgId;
        Data = msgData;
    }

    public UInt16 Id;
    public byte[] Data;
}
