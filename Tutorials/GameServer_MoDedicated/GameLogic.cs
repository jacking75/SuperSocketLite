using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
            Task.Run(() => Update()).ConfigureAwait(false);
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
                MainServer.MainLogger.Debug($"[GameLogic-Update] Call. Index:{Index}");

                if (MsgQueue.TryDequeue(out var gameMsg) == false)
                {
                    MainServer.MainLogger.Debug($"[GameLogic-Update] id: {gameMsg.MsgId}. Index:{Index}");

                    if (gameMsg.MsgId == 0)
                    {
                        return 1;
                    }
                }

                MainServer.MainLogger.Debug($"[GameLogic-Update] Next Frame... Index:{Index}");
                await Task.Delay(33);
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
