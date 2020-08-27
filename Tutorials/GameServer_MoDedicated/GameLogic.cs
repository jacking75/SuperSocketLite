using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
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
namespace GameServer
{
    public class GameLogic
    {
        UInt32 Index = 0;
        bool IsRunable = false;
        ConcurrentQueue<GameMessage> MsgQueue = new ConcurrentQueue<GameMessage>();

        public void Init(UInt32 index)
        {
            Index = index;
        }

        public void Clear()
        {

        }

        public void Start()
        {
            IsRunable = true;
            Task.Run(() => Update());
        }

        public void End()
        {
            IsRunable = false;
        }

        public void AddMessage(UInt16 msgId, byte[] msgData)
        {
            MsgQueue.Enqueue(new GameMessage(msgId, msgData));
        }

        public async Task<int> Update()
        {
            MainServer.MainLogger.Debug($"[GameLogic-Update] Start. Index:{Index}");

            while (IsRunable)
            {
                MainServer.MainLogger.Debug($"[GameLogic-Update] Call. Index:{Index}, [{DateTime.Now.Millisecond}]");

                if (MsgQueue.TryDequeue(out var gameMsg))
                {
                    MainServer.MainLogger.Debug($"[GameLogic-Update] id: {gameMsg.MsgId}. Index:{Index}");

                    if (gameMsg.MsgId == 0)
                    {
                        return 1;
                    }
                }

                //System.Threading.Thread.Sleep(3000);

                //MainServer.MainLogger.Debug($"[GameLogic-Update] Next Frame... Index:{Index}");
                await Task.Delay(100); // 
            }

            MainServer.MainLogger.Debug($"[GameLogic-Update] End. Index:{Index}");
            return 0;
        }
        
    }


    public struct GameMessage
    {
        public GameMessage(UInt16 msgId, byte[] msgData)
        {
            MsgId = msgId;
            MsgData = msgData;
        }

        public UInt16 MsgId;
        public byte[] MsgData;
    }

}
